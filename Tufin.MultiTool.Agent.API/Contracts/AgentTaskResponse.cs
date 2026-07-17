using Tufin.MultiAgentTool.Domain.Tasks;

namespace Tufin.MultiTool.Agent.API.Contracts;

public sealed record AgentTaskResponse(
    Guid TaskId,
    string Input,
    string Model,
    AgentTaskStatus Status,
    string? FinalAnswer,
    string? Error,
    DateTimeOffset CreatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    TaskMetricsResponse Metrics,
    IReadOnlyList<AgentTraceEventResponse> Trace);

public sealed record TaskMetricsResponse(
    long? TotalLatencyMs,
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);