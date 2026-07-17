namespace Tufin.MultiAgentTool.Application.Persistence;

/// <summary>
///     Provides read-only access to persisted task executions.
/// </summary>
public interface IAgentTaskReader
{
    Task<AgentTaskDetails?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<AgentTaskListItem>> GetAllAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int take,
        CancellationToken cancellationToken);
}