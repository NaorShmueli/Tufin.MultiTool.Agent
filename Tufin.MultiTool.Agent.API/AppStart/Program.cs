using System.Diagnostics;
using Tufin.MultiAgentTool.Persistence.Initialization;
using Tufin.MultiAgentTool.Tools.Database;

namespace Tufin.MultiTool.Agent.API.AppStart;

public class Program
{
    public static async Task Main(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;

        var host = CreateHostBuilder(args).Build();

        await InitializeDatabaseAsync(host.Services);

        await host.RunAsync();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
    }

    private static async Task InitializeDatabaseAsync(
        IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseInitialization");

        logger.LogInformation(
            "Starting agent database initialization.");

        var initializer = scope.ServiceProvider
            .GetRequiredService<AgentDatabaseInitializer>();

        await initializer.InitializeAsync();

        logger.LogInformation(
            "Agent database initialization completed successfully.");

        logger.LogInformation(
            "Starting catalog database initialization.");

        var catalogInitializer = scope.ServiceProvider
            .GetRequiredService<CatalogDatabaseInitializer>();

        await catalogInitializer.InitializeAsync();

        logger.LogInformation(
            "Catalog database initialization completed successfully.");
    }
}
