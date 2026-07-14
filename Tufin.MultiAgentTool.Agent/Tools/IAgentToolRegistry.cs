using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Agent.Tools;

public interface IAgentToolRegistry
{
    IReadOnlyCollection<AgentToolDefinition> GetDefinitions();

    bool TryResolve(
        string toolName,
        out IAgentTool? tool);
}