using System.Text.Json;

namespace Tufin.MultiAgentTool.Application.Tools;

/// <summary>
///     A backend capability that can be selected by the language model
///     and executed by deterministic application code.
/// </summary>
public interface IAgentTool
{
    AgentToolDefinition Definition { get; }

    Task<AgentToolExecutionResult> ExecuteAsync(
        JsonElement arguments,
        AgentToolExecutionContext context,
        CancellationToken cancellationToken);
}