using Serilog;

namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    /// <summary>
    ///     Configure logs provider for built-in aspnet core logs
    /// </summary>
    public void ConfigureLogs(IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(_configuration)
            .Enrich.WithThreadId()
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(Log.Logger, true);
        });
    }
}