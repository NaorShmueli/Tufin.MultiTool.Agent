namespace Tufin.MultiAgentTool.Application.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}