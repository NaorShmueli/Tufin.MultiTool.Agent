using Microsoft.EntityFrameworkCore;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Persistence.Database;
using Tufin.MultiAgentTool.Persistence.Mapping;

namespace Tufin.MultiAgentTool.Persistence.Repositories;

public sealed class AgentTaskReader
    : IAgentTaskReader
{
    private readonly AgentDbContext _dbContext;

    public AgentTaskReader(
        AgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<AgentTaskDetails?> GetByIdAsync(
        Guid taskId,
        CancellationToken cancellationToken)
    {
        if (taskId == Guid.Empty)
        {
            return null;
        }

        var entity =
            await _dbContext.Tasks
                .AsNoTracking()
                .Include(task => task.TraceEvents)
                .SingleOrDefaultAsync(
                    task => task.Id == taskId,
                    cancellationToken);

        return entity is null
            ? null
            : AgentPersistenceMapper.ToDetails(entity);
    }

    public async Task<IReadOnlyList<AgentTaskListItem>> GetAllAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int take,
        CancellationToken cancellationToken)
    {
        if (from >= to)
        {
            throw new ArgumentException(
                "'from' must be earlier than 'to'.");
        }

        if (take is < 1 or > 200)
        {
            throw new ArgumentOutOfRangeException(
                nameof(take),
                "Take must be between 1 and 200.");
        }

        var fromUnixMs = from.ToUnixTimeMilliseconds();
        var toUnixMs = to.ToUnixTimeMilliseconds();

        var rows = await _dbContext.Tasks
            .AsNoTracking()
            .Where(task =>
                task.CreatedAtUnixMs >= fromUnixMs &&
                task.CreatedAtUnixMs < toUnixMs)
            .OrderByDescending(task => task.CreatedAtUnixMs)
            .Take(take)
            .Select(task => new
            {
                task.Id,
                task.Input,
                task.Status,
                task.CreatedAtUnixMs
            })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new AgentTaskListItem(
                row.Id,
                row.Input,
                Enum.Parse<AgentTaskStatus>(
                    row.Status,
                    true),
                DateTimeOffset.FromUnixTimeMilliseconds(
                    row.CreatedAtUnixMs)))
            .ToArray();
    }
}