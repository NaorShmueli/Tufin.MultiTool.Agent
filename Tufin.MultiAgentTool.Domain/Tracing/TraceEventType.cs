namespace Tufin.MultiAgentTool.Domain.Tracing;

/// <summary>
///     Describes an observable event that occurred during an agent task.
/// </summary>
public enum TraceEventType
{
    TaskStarted = 0,

    /// <summary>
    ///     The LLM selected the next action or produced a structured decision.
    /// </summary>
    ModelDecision = 1,

    /// <summary>
    ///     The backend is about to execute a tool.
    /// </summary>
    ToolCall = 2,

    /// <summary>
    ///     A tool completed and returned an observation.
    /// </summary>
    ToolResult = 3,

    /// <summary>
    ///     The LLM produced the final user-facing answer.
    /// </summary>
    FinalAnswer = 4,

    TaskCompleted = 5,
    TaskFailed = 6,
    TaskCancelled = 7,
    MaxStepsExceeded = 8
}