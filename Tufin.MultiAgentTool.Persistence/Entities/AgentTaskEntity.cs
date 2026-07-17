namespace Tufin.MultiAgentTool.Persistence.Entities;

internal sealed class AgentTaskEntity
{
    public Guid Id { get; set; }

    public string Input { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? FinalAnswer { get; set; }

    public string? Error { get; set; }

    public long CreatedAtUnixMs { get; set; }

    public long? StartedAtUnixMs { get; set; }

    public long? CompletedAtUnixMs { get; set; }

    public long? TotalLatencyMs { get; set; }

    public int PromptTokens { get; set; }

    public int CompletionTokens { get; set; }

    public List<AgentTraceEventEntity> TraceEvents { get; set; } = [];
}