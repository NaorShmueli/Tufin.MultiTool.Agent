using Microsoft.EntityFrameworkCore;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Persistence.Database;
using Tufin.MultiAgentTool.Persistence.Mapping;

namespace Tufin.MultiAgentTool.Persistence.Repositories;

public sealed class AgentTaskRepository
    : IAgentTaskRepository
{
    private readonly AgentDbContext _dbContext;

    public AgentTaskRepository(
        AgentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        AgentTask task,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);

        var alreadyExists =
            await _dbContext.Tasks.AnyAsync(
                entity => entity.Id == task.Id,
                cancellationToken);

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                $"Task '{task.Id}' already exists.");
        }

        var entity =
            AgentPersistenceMapper.CreateEntity(task);

        _dbContext.Tasks.Add(entity);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task UpdateAsync(
        AgentTask task,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(task);

        var entity =
            await _dbContext.Tasks
                .Include(existing => existing.TraceEvents)
                .SingleOrDefaultAsync(
                    existing => existing.Id == task.Id,
                    cancellationToken);

        if (entity is null)
        {
            throw new InvalidOperationException(
                $"Task '{task.Id}' was not found.");
        }

        AgentPersistenceMapper.ApplyTaskState(
            entity,
            task);

        var persistedTraceIds =
            entity.TraceEvents
                .Select(traceEvent => traceEvent.Id)
                .ToHashSet();

        var newTraceEvents =
            task.TraceEvents
                .Where(traceEvent =>
                    !persistedTraceIds.Contains(
                        traceEvent.Id))
                .Select(
                    AgentPersistenceMapper
                        .CreateTraceEntity)
                .ToArray();

        _dbContext.TraceEvents.AddRange(newTraceEvents);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
        _dbContext.ChangeTracker.Clear();
    }
}