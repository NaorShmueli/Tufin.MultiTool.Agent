using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tracing;

namespace Tufin.MultiAgentTool.Application.Persistence;

public sealed record AgentTraceEventDetails(
    Guid Id,
    int Sequence,
    int StepNumber,
    TraceEventType EventType,
    DateTimeOffset OccurredAt,
    string? DecisionSummary,
    string? ToolName,
    string? ArgumentsJson,
    string? ResultJson,
    long? LatencyMs,
    TokenUsage? TokenUsage,
    string? Error);