namespace Tufin.MultiAgentTool.Application.LanguageModels;

/// <summary>
///     Provider-independent representation of a message
///     exchanged with the language model.
/// </summary>
public sealed class LanguageModelMessage
{
    private LanguageModelMessage(
        LanguageModelRole role,
        string? content,
        IReadOnlyList<LanguageModelToolCall>? toolCalls,
        string? toolCallId,
        string? toolName)
    {
        Role = role;
        Content = content;
        ToolCalls = toolCalls ?? Array.Empty<LanguageModelToolCall>();
        ToolCallId = toolCallId;
        ToolName = toolName;
    }

    public LanguageModelRole Role { get; }

    public string? Content { get; }

    public IReadOnlyList<LanguageModelToolCall> ToolCalls { get; }

    public string? ToolCallId { get; }

    public string? ToolName { get; }

    public static LanguageModelMessage System(string content)
    {
        return CreateTextMessage(
            LanguageModelRole.System,
            content);
    }

    public static LanguageModelMessage User(string content)
    {
        return CreateTextMessage(
            LanguageModelRole.User,
            content);
    }

    public static LanguageModelMessage AssistantText(string content)
    {
        return CreateTextMessage(
            LanguageModelRole.Assistant,
            content);
    }

    public static LanguageModelMessage AssistantToolCalls(
        IReadOnlyList<LanguageModelToolCall> toolCalls)
    {
        ArgumentNullException.ThrowIfNull(toolCalls);

        if (toolCalls.Count == 0)
        {
            throw new ArgumentException(
                "At least one tool call is required.",
                nameof(toolCalls));
        }

        return new LanguageModelMessage(
            LanguageModelRole.Assistant,
            null,
            toolCalls.ToArray(),
            null,
            null);
    }

    public static LanguageModelMessage ToolResult(
        string toolCallId,
        string toolName,
        string resultJson)
    {
        if (string.IsNullOrWhiteSpace(toolCallId))
        {
            throw new ArgumentException(
                "Tool call ID is required.",
                nameof(toolCallId));
        }

        if (string.IsNullOrWhiteSpace(toolName))
        {
            throw new ArgumentException(
                "Tool name is required.",
                nameof(toolName));
        }

        if (string.IsNullOrWhiteSpace(resultJson))
        {
            throw new ArgumentException(
                "Tool result is required.",
                nameof(resultJson));
        }

        return new LanguageModelMessage(
            LanguageModelRole.Tool,
            resultJson,
            null,
            toolCallId.Trim(),
            toolName.Trim());
    }

    private static LanguageModelMessage CreateTextMessage(
        LanguageModelRole role,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Message content is required.",
                nameof(content));
        }

        return new LanguageModelMessage(
            role,
            content.Trim(),
            null,
            null,
            null);
    }
}