namespace Tufin.MultiTool.Agent.API.Contracts;

public sealed record TokenUsageResponse(
    int PromptTokens,
    int CompletionTokens,
    int TotalTokens);