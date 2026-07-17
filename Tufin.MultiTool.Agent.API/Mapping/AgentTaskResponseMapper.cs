using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiTool.Agent.API.Contracts;

namespace Tufin.MultiTool.Agent.API.Mapping;

public static class AgentTaskResponseMapper
{
    public static AgentTaskResponse Map(
        AgentTaskDetails task)
    {
        ArgumentNullException.ThrowIfNull(task);

        var trace = task.Trace
            .OrderBy(traceEvent => traceEvent.Sequence)
            .Select(MapTraceEvent)
            .ToArray();

        return new AgentTaskResponse(
            task.Id,
            task.Input,
            task.Model,
            task.Status,
            task.FinalAnswer,
            task.Error,
            task.CreatedAt,
            task.StartedAt,
            task.CompletedAt,
            new TaskMetricsResponse(
                task.TotalLatencyMs,
                task.TokenUsage.PromptTokens,
                task.TokenUsage.CompletionTokens,
                task.TokenUsage.TotalTokens),
            trace);
    }

    private static AgentTraceEventResponse MapTraceEvent(
        AgentTraceEventDetails traceEvent)
    {
        TokenUsageResponse? tokenUsage = null;

        if (traceEvent.TokenUsage is not null)
        {
            tokenUsage = new TokenUsageResponse(
                traceEvent.TokenUsage.PromptTokens,
                traceEvent.TokenUsage.CompletionTokens,
                traceEvent.TokenUsage.TotalTokens);
        }

        return new AgentTraceEventResponse(
            traceEvent.Id,
            traceEvent.Sequence,
            traceEvent.StepNumber,
            traceEvent.EventType,
            traceEvent.OccurredAt,
            traceEvent.DecisionSummary,
            traceEvent.ToolName,
            ParseJson(traceEvent.ArgumentsJson),
            ParseJson(traceEvent.ResultJson),
            traceEvent.LatencyMs,
            tokenUsage,
            traceEvent.Error);
    }

    private static JToken? ParseJson(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        try
        {
            return JToken.Parse(value);
        }
        catch (JsonReaderException)
        {
            // Defensive fallback for old or malformed trace values.
            return new JValue(value);
        }
    }
}