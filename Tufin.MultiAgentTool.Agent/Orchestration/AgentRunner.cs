using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using Tufin.MultiAgentTool.Agent.Configuration;
using Tufin.MultiAgentTool.Agent.Serialization;
using Tufin.MultiAgentTool.Agent.Tools;
using Tufin.MultiAgentTool.Application.Agents;
using Tufin.MultiAgentTool.Application.LanguageModels;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Application.Time;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Domain.Tracing;
using TraceEventType = Tufin.MultiAgentTool.Domain.Tracing.TraceEventType;

namespace Tufin.MultiAgentTool.Agent.Orchestration;

/// <summary>
/// Executes the observe-decide-act agent loop.
///
/// The language model selects the next action.
/// The backend validates and executes that action.
/// Every observable step is recorded on the AgentTask aggregate.
/// </summary>
public sealed class AgentRunner : IAgentRunner
{
    private readonly ILanguageModelClient _languageModelClient;
    private readonly IAgentToolRegistry _toolRegistry;
    private readonly IAgentTaskRepository _taskRepository;
    private readonly IClock _clock;
    private readonly AgentPromptBuilder _promptBuilder;
    private readonly AgentDecisionSummaryFactory _summaryFactory;
    private readonly AgentJsonSerializer _jsonSerializer;
    private readonly AgentOptions _options;
    private readonly ILogger<AgentRunner> _logger;

    public AgentRunner(
        ILanguageModelClient languageModelClient,
        IAgentToolRegistry toolRegistry,
        IAgentTaskRepository taskRepository,
        IClock clock,
        AgentPromptBuilder promptBuilder,
        AgentDecisionSummaryFactory summaryFactory,
        AgentJsonSerializer jsonSerializer,
        IOptions<AgentOptions> options,
        ILogger<AgentRunner> logger)
    {
        _languageModelClient = languageModelClient;
        _toolRegistry = toolRegistry;
        _taskRepository = taskRepository;
        _clock = clock;
        _promptBuilder = promptBuilder;
        _summaryFactory = summaryFactory;
        _jsonSerializer = jsonSerializer;
        _options = options.Value;
        _logger = logger;

        _options.Validate();
    }

    public async Task RunAsync(
        AgentTask task,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);

        if (task.Status != AgentTaskStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Only pending tasks may be executed. " +
                $"Current status: {task.Status}.");
        }

        var messages = _promptBuilder
            .BuildInitialMessages(task.Input)
            .ToList();

        var toolDefinitions =
            _toolRegistry.GetDefinitions().ToArray();

        try
        {
            task.Start(_clock.UtcNow);

            await PersistAsync(
                task,
                cancellationToken);

            for (var stepNumber = 1;
                 stepNumber <= _options.MaxSteps;
                 stepNumber++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var modelResponse =
                    await CallLanguageModelAsync(
                        task,
                        stepNumber,
                        messages,
                        toolDefinitions,
                        cancellationToken);

                if (modelResponse.HasToolCalls)
                {
                    var shouldContinue =
                        await HandleToolCallsAsync(
                            task,
                            stepNumber,
                            messages,
                            modelResponse,
                            cancellationToken);

                    if (shouldContinue)
                    {
                        continue;
                    }

                    return;
                }

                if (modelResponse.HasFinalContent)
                {
                    task.Complete(
                        modelResponse.Content!,
                        _clock.UtcNow);

                    await PersistAsync(
                        task,
                        cancellationToken);

                    return;
                }

                task.Fail(
                    "The model returned neither a tool call nor a final answer.",
                    _clock.UtcNow);

                await PersistAsync(
                    task,
                    cancellationToken);

                return;
            }

            task.MarkMaxStepsExceeded(
                _options.MaxSteps,
                _clock.UtcNow);

            await PersistAsync(
                task,
                cancellationToken);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            if (task.Status == AgentTaskStatus.Running)
            {
                task.Cancel(
                    _clock.UtcNow,
                    "Task execution was cancelled.");

                await PersistWithoutCancellationAsync(task);
            }

            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Agent task {TaskId} failed unexpectedly.",
                task.Id);

            if (task.Status == AgentTaskStatus.Running)
            {
                task.Fail(
                    exception.Message,
                    _clock.UtcNow);

                await PersistWithoutCancellationAsync(task);
            }
        }
    }

    private async Task<LanguageModelResponse>
        CallLanguageModelAsync(
            AgentTask task,
            int stepNumber,
            IReadOnlyList<LanguageModelMessage> messages,
            IReadOnlyList<AgentToolDefinition> toolDefinitions,
            CancellationToken cancellationToken)
    {
        var request = new LanguageModelRequest(
            messages: messages,
            tools: toolDefinitions,
            temperature: _options.Temperature);

        var response =
            await _languageModelClient.CompleteAsync(
                request,
                cancellationToken);

        task.RecordTrace(
            stepNumber: stepNumber,
            eventType: TraceEventType.ModelDecision,
            occurredAt: _clock.UtcNow,
            decisionSummary:
                _summaryFactory.Create(response),
            latency: response.Latency,
            tokenUsage: response.TokenUsage);

        await PersistAsync(
            task,
            cancellationToken);

        return response;
    }

    private async Task<bool> HandleToolCallsAsync(
        AgentTask task,
        int stepNumber,
        List<LanguageModelMessage> messages,
        LanguageModelResponse modelResponse,
        CancellationToken cancellationToken)
    {
        if (modelResponse.ToolCalls.Count >
            _options.MaxToolCallsPerStep)
        {
            task.Fail(
                $"The model returned " +
                $"{modelResponse.ToolCalls.Count} tool calls, " +
                $"exceeding the configured maximum of " +
                $"{_options.MaxToolCallsPerStep}.",
                _clock.UtcNow);

            await PersistAsync(
                task,
                cancellationToken);

            return false;
        }

        // The assistant tool-call message must be preserved
        // before adding the corresponding tool results.
        messages.Add(
            LanguageModelMessage.AssistantToolCalls(
                modelResponse.ToolCalls));

        foreach (var toolCall in modelResponse.ToolCalls)
        {
            await ExecuteSingleToolCallAsync(
                task,
                stepNumber,
                messages,
                toolCall,
                cancellationToken);
        }

        return true;
    }

    private async Task ExecuteSingleToolCallAsync(
        AgentTask task,
        int stepNumber,
        List<LanguageModelMessage> messages,
        LanguageModelToolCall toolCall,
        CancellationToken cancellationToken)
    {
        var argumentsJson =
            _jsonSerializer.SerializeJsonElement(
                toolCall.Arguments);

        task.RecordTrace(
            stepNumber: stepNumber,
            eventType: TraceEventType.ToolCall,
            occurredAt: _clock.UtcNow,
            decisionSummary:
                $"The backend received a request to execute " +
                $"the '{toolCall.Name}' tool.",
            toolName: toolCall.Name,
            argumentsJson: argumentsJson);

        await PersistAsync(
            task,
            cancellationToken);

        AgentToolExecutionResult executionResult;
        TimeSpan toolLatency;

        if (!_toolRegistry.TryResolve(
                toolCall.Name,
                out var tool) ||
            tool is null)
        {
            executionResult =
                AgentToolExecutionResult.Failure(
                    errorCode: "unknown_tool",
                    errorMessage:
                        $"The tool '{toolCall.Name}' " +
                        "is not registered.");

            toolLatency = TimeSpan.Zero;
        }
        else
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                var executionContext =
                    new AgentToolExecutionContext(task.Id);

                executionResult =
                    await tool.ExecuteAsync(
                        toolCall.Arguments,
                        executionContext,
                        cancellationToken);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Tool {ToolName} failed for task {TaskId}.",
                    toolCall.Name,
                    task.Id);

                executionResult =
                    AgentToolExecutionResult.Failure(
                        errorCode: "tool_execution_error",
                        errorMessage: exception.Message);
            }
            finally
            {
                stopwatch.Stop();
            }

            toolLatency = stopwatch.Elapsed;
        }

        var resultJson =
            _jsonSerializer.SerializeToolResult(
                executionResult);

        task.RecordTrace(
            stepNumber: stepNumber,
            eventType: TraceEventType.ToolResult,
            occurredAt: _clock.UtcNow,
            decisionSummary: executionResult.IsSuccess
                ? $"The '{toolCall.Name}' tool completed successfully."
                : $"The '{toolCall.Name}' tool failed.",
            toolName: toolCall.Name,
            argumentsJson: argumentsJson,
            resultJson: resultJson,
            latency: toolLatency,
            error: executionResult.ErrorMessage);

        await PersistAsync(
            task,
            cancellationToken);

        messages.Add(
            LanguageModelMessage.ToolResult(
                toolCallId: toolCall.Id,
                toolName: toolCall.Name,
                resultJson: resultJson));
    }

    private Task PersistAsync(
        AgentTask task,
        CancellationToken cancellationToken)
    {
        return _taskRepository.UpdateAsync(
            task,
            cancellationToken);
    }

    private async Task PersistWithoutCancellationAsync(
        AgentTask task)
    {
        try
        {
            await _taskRepository.UpdateAsync(
                task,
                CancellationToken.None);
        }
        catch (Exception persistenceException)
        {
            _logger.LogError(
                persistenceException,
                "Failed to persist terminal state for task {TaskId}.",
                task.Id);
        }
    }
}