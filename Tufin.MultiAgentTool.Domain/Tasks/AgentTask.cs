using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tracing;

namespace Tufin.MultiAgentTool.Domain.Tasks;

/// <summary>
/// Aggregate root representing one complete user task
/// and all trace events produced while executing it.
/// </summary>
public sealed class AgentTask
{
    private readonly List<AgentTraceEvent> _traceEvents = [];

    // Required by persistence frameworks such as EF Core.
    private AgentTask()
    {
    }

    private AgentTask(
        Guid id,
        string input,
        string model,
        DateTimeOffset createdAt)
    {
        Id = id;
        Input = input;
        Model = model;
        CreatedAt = createdAt;
        Status = AgentTaskStatus.Pending;
        TokenUsage = TokenUsage.Zero;
    }

    public Guid Id { get; private set; }

    public string Input { get; private set; } = string.Empty;

    public string Model { get; private set; } = string.Empty;

    public AgentTaskStatus Status { get; private set; }

    public string? FinalAnswer { get; private set; }

    public string? Error { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? StartedAt { get; private set; }

    public DateTimeOffset? CompletedAt { get; private set; }

    public long? TotalLatencyMs { get; private set; }

    public TokenUsage TokenUsage { get; private set; } = TokenUsage.Zero;

    public IReadOnlyCollection<AgentTraceEvent> TraceEvents =>
        _traceEvents.AsReadOnly();

    public static AgentTask Create(
        string input,
        string model,
        DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            throw new ArgumentException(
                "Task input is required.",
                nameof(input));
        }

        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException(
                "Model name is required.",
                nameof(model));
        }

        return new AgentTask(
            id: Guid.NewGuid(),
            input: input.Trim(),
            model: model.Trim(),
            createdAt: createdAt);
    }

    public void Start(DateTimeOffset startedAt)
    {
        if (Status != AgentTaskStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Cannot start a task in status {Status}.");
        }

        if (startedAt < CreatedAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(startedAt),
                "Start time cannot be before creation time.");
        }

        Status = AgentTaskStatus.Running;
        StartedAt = startedAt;

        RecordTrace(
            stepNumber: 0,
            eventType: TraceEventType.TaskStarted,
            occurredAt: startedAt,
            decisionSummary: "Task execution started.");
    }

    public AgentTraceEvent RecordTrace(
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
        if (Status is AgentTaskStatus.Pending)
        {
            throw new InvalidOperationException(
                "Cannot record execution events before the task starts.");
        }

        var traceEvent = AgentTraceEvent.Create(
            taskId: Id,
            sequence: _traceEvents.Count + 1,
            stepNumber: stepNumber,
            eventType: eventType,
            occurredAt: occurredAt,
            decisionSummary: decisionSummary,
            toolName: toolName,
            argumentsJson: argumentsJson,
            resultJson: resultJson,
            latency: latency,
            tokenUsage: tokenUsage,
            error: error);

        _traceEvents.Add(traceEvent);

        if (tokenUsage is not null)
        {
            TokenUsage = TokenUsage.Add(tokenUsage);
        }

        return traceEvent;
    }

    public void Complete(
        string finalAnswer,
        DateTimeOffset completedAt)
    {
        EnsureRunning();

        if (string.IsNullOrWhiteSpace(finalAnswer))
        {
            throw new ArgumentException(
                "Final answer is required.",
                nameof(finalAnswer));
        }

        EnsureValidCompletionTime(completedAt);

        RecordTrace(
            stepNumber: GetCurrentStepNumber(),
            eventType: TraceEventType.FinalAnswer,
            occurredAt: completedAt,
            decisionSummary: "The model produced the final user-facing answer.",
            resultJson: finalAnswer);

        FinalAnswer = finalAnswer.Trim();
        Status = AgentTaskStatus.Completed;
        CompletedAt = completedAt;
        TotalLatencyMs = CalculateTotalLatency(completedAt);

        RecordTrace(
            stepNumber: GetCurrentStepNumber(),
            eventType: TraceEventType.TaskCompleted,
            occurredAt: completedAt,
            decisionSummary: "Task completed successfully.");
    }

    public void Fail(
        string error,
        DateTimeOffset failedAt)
    {
        EnsureRunning();

        if (string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException(
                "Failure reason is required.",
                nameof(error));
        }

        EnsureValidCompletionTime(failedAt);

        Error = error.Trim();

        RecordTrace(
            stepNumber: GetCurrentStepNumber(),
            eventType: TraceEventType.TaskFailed,
            occurredAt: failedAt,
            decisionSummary: "Task execution failed.",
            error: Error);

        Status = AgentTaskStatus.Failed;
        CompletedAt = failedAt;
        TotalLatencyMs = CalculateTotalLatency(failedAt);
    }

    public void Cancel(
        DateTimeOffset cancelledAt,
        string? reason = null)
    {
        EnsureRunning();
        EnsureValidCompletionTime(cancelledAt);

        Error = string.IsNullOrWhiteSpace(reason)
            ? "Task was cancelled."
            : reason.Trim();

        RecordTrace(
            stepNumber: GetCurrentStepNumber(),
            eventType: TraceEventType.TaskCancelled,
            occurredAt: cancelledAt,
            decisionSummary: "Task execution was cancelled.",
            error: Error);

        Status = AgentTaskStatus.Cancelled;
        CompletedAt = cancelledAt;
        TotalLatencyMs = CalculateTotalLatency(cancelledAt);
    }

    public void MarkMaxStepsExceeded(
        int maxSteps,
        DateTimeOffset completedAt)
    {
        EnsureRunning();

        if (maxSteps <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxSteps));
        }

        EnsureValidCompletionTime(completedAt);

        Error = $"Agent exceeded the maximum of {maxSteps} steps.";

        RecordTrace(
            stepNumber: maxSteps,
            eventType: TraceEventType.MaxStepsExceeded,
            occurredAt: completedAt,
            decisionSummary: "The agent loop was stopped by the maximum-step guard.",
            error: Error);

        Status = AgentTaskStatus.MaxStepsExceeded;
        CompletedAt = completedAt;
        TotalLatencyMs = CalculateTotalLatency(completedAt);
    }

    private void EnsureRunning()
    {
        if (Status != AgentTaskStatus.Running)
        {
            throw new InvalidOperationException(
                $"The task must be running. Current status: {Status}.");
        }
    }

    private void EnsureValidCompletionTime(
        DateTimeOffset completedAt)
    {
        if (StartedAt is null)
        {
            throw new InvalidOperationException(
                "The task has not started.");
        }

        if (completedAt < StartedAt.Value)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completedAt),
                "Completion time cannot be before start time.");
        }
    }

    private long CalculateTotalLatency(
        DateTimeOffset completedAt)
    {
        return Math.Max(
            0L,
            (long)(completedAt - StartedAt!.Value).TotalMilliseconds);
    }

    private int GetCurrentStepNumber()
    {
        return _traceEvents.Count == 0
            ? 0
            : _traceEvents.Max(traceEvent => traceEvent.StepNumber);
    }
}