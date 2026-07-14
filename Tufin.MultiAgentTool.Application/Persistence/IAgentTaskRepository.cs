using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Persistence;

public interface IAgentTaskRepository
{
    Task AddAsync(
        AgentTask task,
        CancellationToken cancellationToken);

    Task<AgentTask?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken);

    Task UpdateAsync(
        AgentTask task,
        CancellationToken cancellationToken);
}