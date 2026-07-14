using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Agents;

/// <summary>
/// Executes the multi-step reasoning and tool-calling loop
/// for a single AgentTask aggregate.
/// </summary>
public interface IAgentRunner
{
    Task RunAsync(
        AgentTask task,
        CancellationToken cancellationToken);
}