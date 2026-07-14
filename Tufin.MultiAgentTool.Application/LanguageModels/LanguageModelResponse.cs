using Tufin.MultiAgentTool.Domain.Metrics;

namespace Tufin.MultiAgentTool.Application.LanguageModels;

public sealed class LanguageModelResponse
{
    public LanguageModelResponse(
        string model,
        string? content,
        IReadOnlyList<LanguageModelToolCall>? toolCalls,
        TokenUsage tokenUsage,
        TimeSpan latency,
        string? finishReason)
    {
        if (string.IsNullOrWhiteSpace(model))
        {
            throw new ArgumentException(
                "Model name is required.",
                nameof(model));
        }

        ArgumentNullException.ThrowIfNull(tokenUsage);

        Model = model.Trim();
        Content = content;
        ToolCalls = toolCalls?.ToArray()
                    ?? Array.Empty<LanguageModelToolCall>();
        TokenUsage = tokenUsage;
        Latency = latency < TimeSpan.Zero
            ? TimeSpan.Zero
            : latency;
        FinishReason = finishReason;
    }

    public string Model { get; }

    public string? Content { get; }

    public IReadOnlyList<LanguageModelToolCall> ToolCalls { get; }

    public TokenUsage TokenUsage { get; }

    public TimeSpan Latency { get; }

    public string? FinishReason { get; }

    public bool HasToolCalls => ToolCalls.Count > 0;

    public bool HasFinalContent =>
        !string.IsNullOrWhiteSpace(Content) &&
        ToolCalls.Count == 0;
}