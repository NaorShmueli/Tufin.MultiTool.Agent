using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tufin.MultiAgentTool.Application.Persistence;
using Tufin.MultiAgentTool.Persistence.Database;
using Tufin.MultiAgentTool.Persistence.Initialization;
using Tufin.MultiAgentTool.Persistence.Repositories;

namespace Tufin.MultiAgentTool.Persistence.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentPersistence(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString =
            configuration.GetConnectionString(
                "AgentDatabase");

        if (string.IsNullOrWhiteSpace(
                connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'AgentDatabase' is required.");
        }

        services.AddDbContext<AgentDbContext>(
            options =>
            {
                options.UseSqlite(
                    connectionString);
            });

        services.AddScoped<
            IAgentTaskRepository,
            AgentTaskRepository>();

        services.AddScoped<
            IAgentTaskReader,
            AgentTaskReader>();

        services.AddScoped<
            AgentDatabaseInitializer>();

        return services;
    }
}