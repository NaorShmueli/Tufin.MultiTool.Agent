using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Agent.Configuration;
using Tufin.MultiAgentTool.Agent.Orchestration;
using Tufin.MultiAgentTool.Agent.Serialization;
using Tufin.MultiAgentTool.Agent.Tools;
using Tufin.MultiAgentTool.Application.LanguageModels;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Application.Time;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Domain.Tracing;
using Tufin.MultiAgentTool.Tools.Calculator;
using Tufin.MultiAgentTool.Tools.Database;
using Tufin.MultiAgentTool.Tools.UnitConversion;

namespace Tufin.MultiAgentTool.EvaluationTests;

public sealed class AgentRunnerEvaluationTests
{
    [Fact]
    public async Task RunAsync_ForCalculatorTask_ShouldReturnFinalAnswerAndObservableTrace()
    {
        var toolCall = CreateToolCall(
            "calculator",
            new { expression = "2 + 3 * 4" });

        var task = AgentTask.Create(
            "Calculate 2 + 3 * 4.",
            "scripted-eval-model",
            DateTimeOffset.UtcNow);

        var runner = CreateRunner(
            new ScriptedLanguageModelClient(
                ToolCallResponse(toolCall),
                FinalResponse("2 + 3 * 4 equals 14.")),
            new CalculatorTool(new SafeMathExpressionEvaluator()));

        await runner.RunAsync(task, CancellationToken.None);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.Equal("2 + 3 * 4 equals 14.", task.FinalAnswer);
        Assert.Contains(task.TraceEvents, trace => trace.EventType == TraceEventType.ToolCall && trace.ToolName == "calculator");
        Assert.Contains(task.TraceEvents, trace => trace.EventType == TraceEventType.ToolResult && trace.ResultJson!.Contains("14"));
        Assert.True(task.TokenUsage.TotalTokens > 0);
    }

    [Fact]
    public async Task RunAsync_ForUnitConversionTask_ShouldUseConverterAndComplete()
    {
        var toolCall = CreateToolCall(
            "unit_converter",
            new
            {
                value = 10,
                fromUnit = "kilometer",
                toUnit = "mile"
            });

        var task = AgentTask.Create(
            "Convert 10 kilometers to miles.",
            "scripted-eval-model",
            DateTimeOffset.UtcNow);

        var runner = CreateRunner(
            new ScriptedLanguageModelClient(
                ToolCallResponse(toolCall),
                FinalResponse("10 kilometers is about 6.21 miles.")),
            new UnitConverterTool(new UnitConversionService()));

        await runner.RunAsync(task, CancellationToken.None);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.Contains("6.21", task.FinalAnswer);
        Assert.Contains(task.TraceEvents, trace => trace.ToolName == "unit_converter");
        Assert.Contains(task.TraceEvents, trace => trace.EventType == TraceEventType.FinalAnswer);
        Assert.NotNull(task.TotalLatencyMs);
    }

    [Fact]
    public async Task RunAsync_WhenNoToolIsNeeded_ShouldReturnDirectFinalAnswer()
    {
        var task = AgentTask.Create(
            "Say hello.",
            "scripted-eval-model",
            DateTimeOffset.UtcNow);

        var runner = CreateRunner(
            new ScriptedLanguageModelClient(
                FinalResponse("Hello!")),
            new CalculatorTool(new SafeMathExpressionEvaluator()));

        await runner.RunAsync(task, CancellationToken.None);

        Assert.Equal(AgentTaskStatus.Completed, task.Status);
        Assert.Equal("Hello!", task.FinalAnswer);
        Assert.DoesNotContain(task.TraceEvents, trace => trace.EventType == TraceEventType.ToolCall);
        Assert.Contains(task.TraceEvents, trace => trace.EventType == TraceEventType.ModelDecision);
        Assert.Contains(task.TraceEvents, trace => trace.EventType == TraceEventType.TaskCompleted);
    }

    [Fact]
    public async Task RunAsync_ForCatalogQuestion_ShouldUseDatabaseQueryTool()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"catalog-eval-{Guid.NewGuid():N}.db");

        try
        {
            var catalogOptions = Options.Create(
                new CatalogDatabaseOptions
                {
                    ConnectionString = $"Data Source={databasePath}"
                });

            await new CatalogDatabaseInitializer(catalogOptions)
                .InitializeAsync();

            var toolCall = CreateToolCall(
                "database_query",
                new
                {
                    query = "SELECT name, price FROM products WHERE name = 'iPhone 17'"
                });

            var task = AgentTask.Create(
                "What is the cost of iPhone 17?",
                "scripted-eval-model",
                DateTimeOffset.UtcNow);

            var runner = CreateRunner(
                new ScriptedLanguageModelClient(
                    ToolCallResponse(toolCall),
                    FinalResponse("The iPhone 17 costs $1,199.")),
                new DatabaseQueryTool(catalogOptions));

            await runner.RunAsync(task, CancellationToken.None);

            Assert.Equal(AgentTaskStatus.Completed, task.Status);
            Assert.Contains("1,199", task.FinalAnswer);
            Assert.Contains(task.TraceEvents, trace => trace.ToolName == "database_query");
            Assert.Contains(
                task.TraceEvents,
                trace =>
                    trace.EventType == TraceEventType.ToolResult &&
                    trace.ResultJson is not null &&
                    trace.ResultJson.Contains("iPhone 17"));
            Assert.True(task.TokenUsage.TotalTokens > 0);
        }
        finally
        {
            TryDelete(databasePath);
            TryDelete(databasePath + "-shm");
            TryDelete(databasePath + "-wal");
        }
    }

    [Fact]
    public async Task RunAsync_WhenModelWritesToolCallAsContent_ShouldExecuteToolInsteadOfCompleting()
    {
        var databasePath = Path.Combine(
            Path.GetTempPath(),
            $"catalog-content-tool-{Guid.NewGuid():N}.db");

        try
        {
            var catalogOptions = Options.Create(
                new CatalogDatabaseOptions
                {
                    ConnectionString = $"Data Source={databasePath}"
                });

            await new CatalogDatabaseInitializer(catalogOptions)
                .InitializeAsync();

            var contentToolCall =
                "{\"name\": \"database_query\", \"parameters\": {\"query\": \"SELECT price FROM products WHERE name = \"iPhone 17\"}}";

            var task = AgentTask.Create(
                "What is the cost of iPhone 17?",
                "scripted-eval-model",
                DateTimeOffset.UtcNow);

            var runner = CreateRunner(
                new ScriptedLanguageModelClient(
                    FinalResponse(contentToolCall),
                    FinalResponse("The iPhone 17 costs $1,199.")),
                new DatabaseQueryTool(catalogOptions));

            await runner.RunAsync(task, CancellationToken.None);

            Assert.Equal(AgentTaskStatus.Completed, task.Status);
            Assert.Equal("The iPhone 17 costs $1,199.", task.FinalAnswer);
            Assert.Contains(task.TraceEvents, trace => trace.ToolName == "database_query");
            Assert.DoesNotContain(
                task.TraceEvents,
                trace => trace.EventType == TraceEventType.FinalAnswer &&
                         trace.ResultJson == contentToolCall);
        }
        finally
        {
            TryDelete(databasePath);
            TryDelete(databasePath + "-shm");
            TryDelete(databasePath + "-wal");
        }
    }

    private static AgentRunner CreateRunner(
        ILanguageModelClient languageModelClient,
        params IAgentTool[] tools)
    {
        return new AgentRunner(
            languageModelClient,
            new AgentToolRegistry(tools),
            new NoOpTaskRepository(),
            new SystemClock(),
            new AgentPromptBuilder(),
            new AgentDecisionSummaryFactory(),
            new AgentJsonSerializer(),
            Options.Create(new AgentOptions
            {
                MaxSteps = 4,
                MaxToolCallsPerStep = 2,
                Temperature = 0
            }),
            NullLogger<AgentRunner>.Instance);
    }

    private static LanguageModelResponse ToolCallResponse(
        LanguageModelToolCall toolCall)
    {
        return new LanguageModelResponse(
            "scripted-eval-model",
            null,
            new[] { toolCall },
            new TokenUsage(20, 5),
            TimeSpan.FromMilliseconds(2),
            "tool_calls");
    }

    private static LanguageModelResponse FinalResponse(
        string content)
    {
        return new LanguageModelResponse(
            "scripted-eval-model",
            content,
            Array.Empty<LanguageModelToolCall>(),
            new TokenUsage(15, 10),
            TimeSpan.FromMilliseconds(2),
            "stop");
    }

    private static LanguageModelToolCall CreateToolCall(
        string name,
        object arguments)
    {
        return new LanguageModelToolCall(
            Guid.NewGuid().ToString("N"),
            name,
            JsonSerializer.SerializeToElement(arguments));
    }

    private sealed class ScriptedLanguageModelClient : ILanguageModelClient
    {
        private readonly Queue<LanguageModelResponse> _responses;

        public ScriptedLanguageModelClient(
            params LanguageModelResponse[] responses)
        {
            _responses = new Queue<LanguageModelResponse>(responses);
        }

        public string ModelName => "scripted-eval-model";

        public Task<LanguageModelResponse> CompleteAsync(
            LanguageModelRequest request,
            CancellationToken cancellationToken)
        {
            if (_responses.Count == 0)
            {
                throw new InvalidOperationException(
                    "No scripted language-model response is available.");
            }

            return Task.FromResult(_responses.Dequeue());
        }
    }

    private sealed class NoOpTaskRepository : IAgentTaskRepository
    {
        public Task AddAsync(
            AgentTask task,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task UpdateAsync(
            AgentTask task,
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class SystemClock : IClock
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
