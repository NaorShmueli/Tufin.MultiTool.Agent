namespace Tufin.MultiAgentTool.Domain.Metrics;

/// <summary>
/// Represents token consumption for a single model call
/// or for the complete task.
/// </summary>
public sealed record TokenUsage
{
    public static TokenUsage Zero { get; } = new(0, 0);

    public int PromptTokens { get; }

    public int CompletionTokens { get; }

    public int TotalTokens => PromptTokens + CompletionTokens;

    public TokenUsage(
        int promptTokens,
        int completionTokens)
    {
        if (promptTokens < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(promptTokens),
                "Prompt token count cannot be negative.");
        }

        if (completionTokens < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(completionTokens),
                "Completion token count cannot be negative.");
        }

        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
    }

    public TokenUsage Add(TokenUsage other)
    {
        ArgumentNullException.ThrowIfNull(other);

        return new TokenUsage(
            PromptTokens + other.PromptTokens,
            CompletionTokens + other.CompletionTokens);
    }
}