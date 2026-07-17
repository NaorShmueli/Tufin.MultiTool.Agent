namespace Tufin.MultiAgentTool.Domain.Tasks;

/// <summary>
///     Represents the lifecycle of an agent task.
/// </summary>
public enum AgentTaskStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    MaxStepsExceeded = 5
}