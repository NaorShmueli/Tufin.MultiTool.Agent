using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Tufin.MultiTool.Agent.API.HealthChecks;

namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    private void ConfigureHealthChecks(IServiceCollection services)
    {
        services.AddHttpClient<OllamaHealthCheck>();

        services
            .AddHealthChecks()
            .AddCheck<AgentDatabaseHealthCheck>(
                "agent_database")
            .AddCheck<OllamaHealthCheck>(
                "ollama");
    }

    private static async Task WriteHealthCheckResponse(
        HttpContext httpContext,
        HealthReport report)
    {
        httpContext.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDurationMs = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                durationMs = entry.Value.Duration.TotalMilliseconds,
                error = entry.Value.Exception?.Message
            })
        };

        await httpContext.Response.WriteAsync(
            JsonSerializer.Serialize(
                response,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                }));
    }
}
