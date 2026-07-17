using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Persistence;

/// <summary>
///     Lightweight read model used for listing previously executed tasks
///     without loading their complete trace.
/// </summary>
public sealed record AgentTaskListItem(
    Guid TaskId,
    string Input,
    AgentTaskStatus Status,
    DateTimeOffset CreatedAt);