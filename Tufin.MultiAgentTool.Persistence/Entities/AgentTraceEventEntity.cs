namespace Tufin.MultiAgentTool.Persistence.Entities;

internal sealed class AgentTraceEventEntity
{
    public Guid Id { get; set; }

    public Guid TaskId { get; set; }

    public int Sequence { get; set; }

    public int StepNumber { get; set; }

    public string EventType { get; set; } = string.Empty;

    public long OccurredAtUnixMs { get; set; }

    public string? DecisionSummary { get; set; }

    public string? ToolName { get; set; }

    public string? ArgumentsJson { get; set; }

    public string? ResultJson { get; set; }

    public long? LatencyMs { get; set; }

    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public string? Error { get; set; }
}