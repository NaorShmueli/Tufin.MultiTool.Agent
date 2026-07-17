using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Persistence;

/// <summary>
///     Persists changes to the AgentTask aggregate.
/// </summary>
public interface IAgentTaskRepository
{
    Task AddAsync(
        AgentTask task,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        AgentTask task,
        CancellationToken cancellationToken);
}