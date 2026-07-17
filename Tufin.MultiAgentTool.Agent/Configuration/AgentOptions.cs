namespace Tufin.MultiAgentTool.Agent.Configuration;

/// <summary>
///     Configuration of the agent execution loop.
/// </summary>
public sealed class AgentOptions
{
    public const string SectionName = "Agent";

    /// <summary>
    ///     Maximum number of LLM decision iterations per task.
    ///     Prevents infinite agent loops.
    /// </summary>
    public int MaxSteps { get; set; } = 8;

    /// <summary>
    ///     Maximum number of tool calls accepted from one model response.
    ///     Independent tool calls may be returned together by some models.
    /// </summary>
    public int MaxToolCallsPerStep { get; set; } = 4;

    /// <summary>
    ///     Low temperature is preferred because tool selection
    ///     should be stable rather than creative.
    /// </summary>
    public double Temperature { get; set; } = 0;

    public void Validate()
    {
        if (MaxSteps <= 0)
        {
            throw new InvalidOperationException(
                "Agent MaxSteps must be greater than zero.");
        }

        if (MaxToolCallsPerStep <= 0)
        {
            throw new InvalidOperationException(
                "Agent MaxToolCallsPerStep must be greater than zero.");
        }

        if (Temperature is < 0 or > 2)
        {
            throw new InvalidOperationException(
                "Agent Temperature must be between 0 and 2.");
        }
    }
}