using Tufin.MultiAgentTool.Domain.Metrics;

namespace Tufin.MultiAgentTool.Domain.Tracing;

/// <summary>
/// A single structured and persisted observation from an agent execution.
/// </summary>
public sealed class AgentTraceEvent
{
    // Required by persistence frameworks such as EF Core.
    private AgentTraceEvent()
    {
    }

    private AgentTraceEvent(
        Guid id,
        Guid taskId,
        int sequence,
        int stepNumber,
        TraceEventType eventType,
        DateTimeOffset occurredAt,
        string? decisionSummary,
        string? toolName,
        string? argumentsJson,
        string? resultJson,
        long? latencyMs,
        TokenUsage? tokenUsage,
        string? error)
    {
        Id = id;
        TaskId = taskId;
        Sequence = sequence;
        StepNumber = stepNumber;
        EventType = eventType;
        OccurredAt = occurredAt;
        DecisionSummary = decisionSummary;
        ToolName = toolName;
        ArgumentsJson = argumentsJson;
        ResultJson = resultJson;
        LatencyMs = latencyMs;
        TokenUsage = tokenUsage;
        Error = error;
    }

    public Guid Id { get; private set; }

    public Guid TaskId { get; private set; }

    /// <summary>
    /// Absolute event order inside the task.
    /// </summary>
    public int Sequence { get; private set; }

    /// <summary>
    /// Agent loop iteration that produced this event.
    /// Multiple events may belong to the same step.
    /// </summary>
    public int StepNumber { get; private set; }

    public TraceEventType EventType { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }

    /// <summary>
    /// A concise, user-safe explanation of the model's decision.
    /// This is not raw chain-of-thought.
    /// </summary>
    public string? DecisionSummary { get; private set; }

    public string? ToolName { get; private set; }

    /// <summary>
    /// Serialized structured tool arguments.
    /// </summary>
    public string? ArgumentsJson { get; private set; }

    /// <summary>
    /// Serialized structured tool result.
    /// </summary>
    public string? ResultJson { get; private set; }

    public long? LatencyMs { get; private set; }

    public TokenUsage? TokenUsage { get; private set; }

    public string? Error { get; private set; }

    internal static AgentTraceEvent Create(
        Guid taskId,
        int sequence,
        int stepNumber,
        TraceEventType eventType,
        DateTimeOffset occurredAt,
        string? decisionSummary = null,
        string? toolName = null,
        string? argumentsJson = null,
        string? resultJson = null,
        TimeSpan? latency = null,
        TokenUsage? tokenUsage = null,
        string? error = null)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException(
                "Task ID cannot be empty.",
                nameof(taskId));
        }

        if (sequence <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sequence),
                "Sequence must be greater than zero.");
        }

        if (stepNumber < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stepNumber),
                "Step number cannot be negative.");
        }

  

        var latencyMs = latency.HasValue
            ? (long?)Math.Max(0L, (long)latency.Value.TotalMilliseconds)
            : null;

        return new AgentTraceEvent(
            id: Guid.NewGuid(),
            taskId: taskId,
            sequence: sequence,
            stepNumber: stepNumber,
            eventType: eventType,
            occurredAt: occurredAt,
            decisionSummary: decisionSummary,
            toolName: toolName,
            argumentsJson: argumentsJson,
            resultJson: resultJson,
            latencyMs: latencyMs,
            tokenUsage: tokenUsage,
            error: error);
    }
}