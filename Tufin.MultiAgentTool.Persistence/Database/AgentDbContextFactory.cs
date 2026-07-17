using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Tufin.MultiAgentTool.Persistence.Database;

public sealed class AgentDbContextFactory
    : IDesignTimeDbContextFactory<AgentDbContext>
{
    public AgentDbContext CreateDbContext(
        string[] args)
    {
        var optionsBuilder =
            new DbContextOptionsBuilder<AgentDbContext>();

        optionsBuilder.UseSqlite(
            "Data Source=agent-observability.design.db");

        return new AgentDbContext(
            optionsBuilder.Options);
    }
}