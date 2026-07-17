using Tufin.MultiAgentTool.Application.Agents;
using Tufin.MultiAgentTool.Application.LanguageModels;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Application.Time;
using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Tasks;

/// <summary>
///     Creates, persists and executes one user task.
/// </summary>
public sealed class SubmitAgentTaskService
    : ISubmitAgentTaskService
{
    private const int MaximumTaskLength = 10_000;
    private readonly IAgentRunner _agentRunner;
    private readonly IClock _clock;

    private readonly ILanguageModelClient _languageModelClient;
    private readonly IAgentTaskReader _taskReader;
    private readonly IAgentTaskRepository _taskRepository;

    public SubmitAgentTaskService(
        ILanguageModelClient languageModelClient,
        IAgentTaskRepository taskRepository,
        IAgentTaskReader taskReader,
        IAgentRunner agentRunner,
        IClock clock)
    {
        _languageModelClient = languageModelClient;
        _taskRepository = taskRepository;
        _taskReader = taskReader;
        _agentRunner = agentRunner;
        _clock = clock;
    }

    public async Task<AgentTaskDetails> SubmitAsync(
        string input,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(
                "Task input is required.",
                nameof(input));
        }

        var normalizedInput = input.Trim();

        if (normalizedInput.Length > MaximumTaskLength)
        {
            throw new ArgumentException(
                $"Task input cannot exceed " +
                $"{MaximumTaskLength} characters.",
                nameof(input));
        }

        var task = AgentTask.Create(
            normalizedInput,
            _languageModelClient.ModelName,
            _clock.UtcNow);

        // The task must exist before AgentRunner starts updating it.
        await _taskRepository.AddAsync(
            task,
            cancellationToken);

        await _agentRunner.RunAsync(
            task,
            cancellationToken);

        // Read the response from persistence rather than returning
        // the in-memory aggregate. This proves that the stored trace
        // is complete and retrievable.
        var persistedTask =
            await _taskReader.GetByIdAsync(
                task.Id,
                cancellationToken);

        return persistedTask
               ?? throw new InvalidOperationException(
                   $"Task '{task.Id}' could not be read " +
                   "after execution.");
    }
}