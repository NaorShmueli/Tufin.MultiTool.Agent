using System.Diagnostics;
using Tufin.MultiAgentTool.Persistence.Initialization;

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
            "Starting database migration.");

        var initializer = scope.ServiceProvider
            .GetRequiredService<AgentDatabaseInitializer>();

        await initializer.InitializeAsync();

        logger.LogInformation(
            "Database migration completed successfully.");
    }
}