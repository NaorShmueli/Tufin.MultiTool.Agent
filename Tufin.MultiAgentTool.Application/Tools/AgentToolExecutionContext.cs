namespace Tufin.MultiAgentTool.Application.Tools;

/// <summary>
///     Trusted backend context passed to a tool.
///     Values in this object are created by the backend,
///     not supplied or selected by the LLM.
/// </summary>
public sealed class AgentToolExecutionContext
{
    public AgentToolExecutionContext(Guid taskId)
    {
        if (taskId == Guid.Empty)
        {
            throw new ArgumentException(
                "Task ID cannot be empty.",
                nameof(taskId));
        }

        TaskId = taskId;
    }

    public Guid TaskId { get; }
}