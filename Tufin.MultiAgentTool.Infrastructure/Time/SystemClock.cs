using Tufin.MultiAgentTool.Application.Time;

namespace Tufin.MultiAgentTool.Infrastructure.Time;

public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow =>
        DateTimeOffset.UtcNow;
}