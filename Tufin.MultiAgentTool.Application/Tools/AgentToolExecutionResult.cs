using System.Text.Json;

namespace Tufin.MultiAgentTool.Application.Tools;

/// <summary>
///     Structured outcome of a real tool execution.
/// </summary>
public sealed class AgentToolExecutionResult
{
    private AgentToolExecutionResult(
        bool isSuccess,
        JsonElement? output,
        string? errorCode,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Output = output?.Clone();
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public JsonElement? Output { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static AgentToolExecutionResult Success(
        JsonElement output)
    {
        return new AgentToolExecutionResult(
            true,
            output,
            null,
            null);
    }

    public static AgentToolExecutionResult Failure(
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException(
                "Error code is required.",
                nameof(errorCode));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));
        }

        return new AgentToolExecutionResult(
            false,
            null,
            errorCode.Trim(),
            errorMessage.Trim());
    }
}