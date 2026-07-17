using Newtonsoft.Json.Linq;
using Tufin.MultiAgentTool.Domain.Tracing;

namespace Tufin.MultiTool.Agent.API.Contracts;

public sealed record AgentTraceEventResponse(
    Guid Id,
    int Sequence,
    int StepNumber,
    TraceEventType EventType,
    DateTimeOffset OccurredAt,
    string? DecisionSummary,
    string? ToolName,
    JToken? Arguments,
    JToken? Result,
    long? LatencyMs,
    TokenUsageResponse? TokenUsage,
    string? Error);