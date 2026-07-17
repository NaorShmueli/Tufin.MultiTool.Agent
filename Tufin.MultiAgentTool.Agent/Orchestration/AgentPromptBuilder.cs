using Tufin.MultiAgentTool.Application.LanguageModels;

namespace Tufin.MultiAgentTool.Agent.Orchestration;

/// <summary>
///     Creates the stable instructions that control agent behavior.
///     Tool schemas themselves are supplied separately in the model request.
/// </summary>
public sealed class AgentPromptBuilder
{
    private const string SystemPrompt =
        @"
        You are a general-purpose AI agent that solves user tasks by using
        the backend tools made available to you.

        Rules:
        1. Use tools whenever current, calculated, converted, searched,
           or database-backed information is required.
        2. Never invent a tool result.
        3. Never claim that a tool was executed unless a tool result message
           was actually provided.
        4. Select only tools included in the provided tool definitions.
        5. Supply arguments that conform exactly to the tool JSON schema.
        6. Tool failures are observations. You may retry with corrected
           arguments or explain the failure, but do not invent success.
        7. Use the smallest number of tool calls needed to solve the task.
        8. After all required tool results are available, produce a clear
           final answer for the user.
        9. Do not expose a raw tool payload as the final response. Interpret
           and summarize the result in natural language.
        10. Do not reveal hidden chain-of-thought. Tool calls, concise
            decision summaries, and observable results are sufficient.

        You may either:
        - request one or more tools; or
        - return the final user-facing answer.

        Do not return a final answer while required factual information is
        still missing.
        ";

    public IReadOnlyList<LanguageModelMessage> BuildInitialMessages(
        string userTask)
    {
        if (string.IsNullOrWhiteSpace(userTask))
        {
            throw new ArgumentException(
                "User task is required.",
                nameof(userTask));
        }

        return
        [
            LanguageModelMessage.System(SystemPrompt),
            LanguageModelMessage.User(userTask.Trim())
        ];
    }
}