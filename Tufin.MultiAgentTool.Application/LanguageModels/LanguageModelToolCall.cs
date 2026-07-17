using System.Text.Json;

namespace Tufin.MultiAgentTool.Application.LanguageModels;

/// <summary>
///     A structured request produced by the language model
///     to execute one backend tool.
/// </summary>
public sealed class LanguageModelToolCall
{
    public LanguageModelToolCall(
        string id,
        string name,
        JsonElement arguments)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException(
                "Tool call ID is required.",
                nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tool name is required.",
                nameof(name));
        }

        if (arguments.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                "Tool arguments must be a JSON object.",
                nameof(arguments));
        }

        Id = id.Trim();
        Name = name.Trim();
        Arguments = arguments.Clone();
    }

    public string Id { get; }

    public string Name { get; }

    public JsonElement Arguments { get; }
}