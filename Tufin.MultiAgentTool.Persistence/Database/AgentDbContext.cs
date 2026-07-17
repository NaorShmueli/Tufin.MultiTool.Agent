using Microsoft.EntityFrameworkCore;
using Tufin.MultiAgentTool.Persistence.Entities;

namespace Tufin.MultiAgentTool.Persistence.Database;

public sealed class AgentDbContext : DbContext
{
    public AgentDbContext(
        DbContextOptions<AgentDbContext> options)
        : base(options)
    {
    }

    internal DbSet<AgentTaskEntity> Tasks =>
        Set<AgentTaskEntity>();

    internal DbSet<AgentTraceEventEntity> TraceEvents =>
        Set<AgentTraceEventEntity>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AgentDbContext).Assembly);
    }
}