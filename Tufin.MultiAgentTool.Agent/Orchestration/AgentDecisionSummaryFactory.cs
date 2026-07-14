using Tufin.MultiAgentTool.Application.LanguageModels;

namespace Tufin.MultiAgentTool.Agent.Orchestration;

public sealed class AgentDecisionSummaryFactory
{
    public string Create(LanguageModelResponse response)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (response.HasToolCalls)
        {
            var toolNames = response.ToolCalls
                .Select(toolCall => toolCall.Name)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            return toolNames.Length == 1
                ? $"The model selected the '{toolNames[0]}' tool."
                : $"The model selected the following tools: " +
                  string.Join(", ", toolNames) + ".";
        }

        if (response.HasFinalContent)
        {
            return "The model produced the final user-facing answer.";
        }

        return "The model returned no executable tool call or final answer.";
    }
}