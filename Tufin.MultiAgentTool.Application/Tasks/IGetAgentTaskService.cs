using Tufin.MultiAgentTool.Application.Persistence;

namespace Tufin.MultiAgentTool.Application.Tasks;

public interface IGetAgentTaskService
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