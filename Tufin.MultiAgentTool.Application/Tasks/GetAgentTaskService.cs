using Tufin.MultiAgentTool.Application.Persistence;

namespace Tufin.MultiAgentTool.Application.Tasks;

public sealed class GetAgentTaskService
    : IGetAgentTaskService
{
    private readonly IAgentTaskReader _taskReader;

    public GetAgentTaskService(
        IAgentTaskReader taskReader)
    {
        _taskReader = taskReader;
    }

    public Task<AgentTaskDetails?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        if (taskId == Guid.Empty)
        {
            return Task.FromResult<AgentTaskDetails?>(null);
        }

        return _taskReader.GetByIdAsync(
            taskId,
            cancellationToken);
    }

    public Task<IReadOnlyList<AgentTaskListItem>> GetAllAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int take,
        CancellationToken cancellationToken)
    {
        return _taskReader.GetAllAsync(
            from,
            to,
            take,
            cancellationToken);
    }
}