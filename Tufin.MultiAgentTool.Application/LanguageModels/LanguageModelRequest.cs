using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Application.LanguageModels;

public sealed class LanguageModelRequest
{
    public LanguageModelRequest(
        IReadOnlyList<LanguageModelMessage> messages,
        IReadOnlyList<AgentToolDefinition> tools,
        double temperature = 0)
    {
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(tools);

        if (messages.Count == 0)
        {
            throw new ArgumentException(
                "At least one message is required.",
                nameof(messages));
        }

        if (temperature is < 0 or > 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(temperature),
                "Temperature must be between 0 and 2.");
        }

        Messages = messages.ToArray();
        Tools = tools.ToArray();
        Temperature = temperature;
    }

    public IReadOnlyList<LanguageModelMessage> Messages { get; }

    public IReadOnlyList<AgentToolDefinition> Tools { get; }

    public double Temperature { get; }
}