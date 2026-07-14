using System.Text.Json;

namespace Tufin.MultiAgentTool.Application.Tools;

/// <summary>
/// Describes a capability that may be selected by the language model.
///
/// The definition is sent to the LLM so it knows:
/// - what the tool does;
/// - when it should use it;
/// - which arguments it must provide.
/// </summary>
public sealed class AgentToolDefinition
{
    public AgentToolDefinition(
        string name,
        string description,
        JsonElement inputSchema)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Tool name is required.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Tool description is required.",
                nameof(description));
        }

        if (inputSchema.ValueKind != JsonValueKind.Object)
        {
            throw new ArgumentException(
                "Tool input schema must be a JSON object.",
                nameof(inputSchema));
        }

        Name = name.Trim();
        Description = description.Trim();

        // JsonElement can point to a JsonDocument owned elsewhere.
        // Clone creates an independent and safe copy.
        InputSchema = inputSchema.Clone();
    }

    public string Name { get; }

    public string Description { get; }

    public JsonElement InputSchema { get; }
}