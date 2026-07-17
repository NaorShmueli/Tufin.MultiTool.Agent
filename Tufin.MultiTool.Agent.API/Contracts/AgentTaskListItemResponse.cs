using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiTool.Agent.API.Contracts;

/// <summary>
///     Represents one task in the task history endpoint.
/// </summary>
public sealed record AgentTaskListItemResponse(
    Guid TaskId,
    string Task,
    AgentTaskStatus Status,
    DateTimeOffset CreatedAt);