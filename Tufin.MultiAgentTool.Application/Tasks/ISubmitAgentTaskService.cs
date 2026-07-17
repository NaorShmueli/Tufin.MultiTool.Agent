using Tufin.MultiAgentTool.Application.Persistence;

namespace Tufin.MultiAgentTool.Application.Tasks;

public interface ISubmitAgentTaskService
{
    Task<AgentTaskDetails> SubmitAsync(
        string input,
        CancellationToken cancellationToken);
}