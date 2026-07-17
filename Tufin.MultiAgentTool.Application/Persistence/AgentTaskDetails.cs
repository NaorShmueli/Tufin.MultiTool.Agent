using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiAgentTool.Application.Persistence;

public sealed record AgentTaskDetails(
    Guid Id,
    string Input,
    string Model,
    AgentTaskStatus Status,
    string? FinalAnswer,
    string? Error,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    long? TotalLatencyMs,
    TokenUsage TokenUsage,
    IReadOnlyList<AgentTraceEventDetails> Trace);