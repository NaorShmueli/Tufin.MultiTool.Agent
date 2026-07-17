using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Domain.Tracing;
using Tufin.MultiAgentTool.Persistence.Entities;

namespace Tufin.MultiAgentTool.Persistence.Mapping;

internal static class AgentPersistenceMapper
{
    public static AgentTaskEntity CreateEntity(
        AgentTask task)
    {
        ArgumentNullException.ThrowIfNull(task);

        var entity = new AgentTaskEntity
        {
            Id = task.Id
        };

        ApplyTaskState(entity, task);

        foreach (var traceEvent in task.TraceEvents)
        {
            entity.TraceEvents.Add(
                CreateTraceEntity(traceEvent));
        }

        return entity;
    }

    public static void ApplyTaskState(
        AgentTaskEntity entity,
        AgentTask task)
    {
        entity.Input = task.Input;
        entity.Model = task.Model;
        entity.Status = task.Status.ToString();
        entity.FinalAnswer = task.FinalAnswer;
        entity.Error = task.Error;

        entity.CreatedAtUnixMs =
            task.CreatedAt.ToUnixTimeMilliseconds();

        entity.StartedAtUnixMs =
            task.StartedAt?.ToUnixTimeMilliseconds();

        entity.CompletedAtUnixMs =
            task.CompletedAt?.ToUnixTimeMilliseconds();

        entity.TotalLatencyMs = task.TotalLatencyMs;

        entity.PromptTokens =
            task.TokenUsage.PromptTokens;

        entity.CompletionTokens =
            task.TokenUsage.CompletionTokens;
    }

    public static AgentTraceEventEntity CreateTraceEntity(
        AgentTraceEvent traceEvent)
    {
        return new AgentTraceEventEntity
        {
            Id = traceEvent.Id,
            TaskId = traceEvent.TaskId,
            Sequence = traceEvent.Sequence,
            StepNumber = traceEvent.StepNumber,
            EventType = traceEvent.EventType.ToString(),

            OccurredAtUnixMs =
                traceEvent.OccurredAt.ToUnixTimeMilliseconds(),

            DecisionSummary =
                traceEvent.DecisionSummary,

            ToolName =
                traceEvent.ToolName,

            ArgumentsJson =
                traceEvent.ArgumentsJson,

            ResultJson =
                traceEvent.ResultJson,

            LatencyMs =
                traceEvent.LatencyMs,

            PromptTokens =
                traceEvent.TokenUsage?.PromptTokens,

            CompletionTokens =
                traceEvent.TokenUsage?.CompletionTokens,

            Error =
                traceEvent.Error
        };
    }

    public static AgentTaskDetails ToDetails(
        AgentTaskEntity entity)
    {
        var status = ParseEnum<AgentTaskStatus>(
            entity.Status,
            nameof(entity.Status));

        var trace = entity.TraceEvents
            .OrderBy(traceEvent => traceEvent.Sequence)
            .Select(ToTraceDetails)
            .ToArray();

        return new AgentTaskDetails(
            entity.Id,
            entity.Input,
            entity.Model,
            status,
            entity.FinalAnswer,
            entity.Error,
            DateTimeOffset.FromUnixTimeMilliseconds(
                entity.CreatedAtUnixMs),
            FromOptionalUnixMilliseconds(
                entity.StartedAtUnixMs),
            FromOptionalUnixMilliseconds(
                entity.CompletedAtUnixMs),
            entity.TotalLatencyMs,
            new TokenUsage(
                entity.PromptTokens,
                entity.CompletionTokens),
            trace);
    }

    private static AgentTraceEventDetails ToTraceDetails(
        AgentTraceEventEntity entity)
    {
        var eventType = ParseEnum<TraceEventType>(
            entity.EventType,
            nameof(entity.EventType));

        TokenUsage? tokenUsage = null;

        if (entity.PromptTokens.HasValue ||
            entity.CompletionTokens.HasValue)
        {
            tokenUsage = new TokenUsage(
                entity.PromptTokens ?? 0,
                entity.CompletionTokens ?? 0);
        }

        return new AgentTraceEventDetails(
            entity.Id,
            entity.Sequence,
            entity.StepNumber,
            eventType,
            DateTimeOffset.FromUnixTimeMilliseconds(
                entity.OccurredAtUnixMs),
            entity.DecisionSummary,
            entity.ToolName,
            entity.ArgumentsJson,
            entity.ResultJson,
            entity.LatencyMs,
            tokenUsage,
            entity.Error);
    }

    private static DateTimeOffset?
        FromOptionalUnixMilliseconds(long? value)
    {
        return value.HasValue
            ? DateTimeOffset.FromUnixTimeMilliseconds(
                value.Value)
            : null;
    }

    private static TEnum ParseEnum<TEnum>(
        string value,
        string propertyName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(
                value,
                false,
                out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException(
            $"Persisted value '{value}' is not a valid " +
            $"{typeof(TEnum).Name} for '{propertyName}'.");
    }
}