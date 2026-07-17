using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Tufin.MultiTool.Agent.API.HealthChecks;

public sealed class AgentDatabaseHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;

    public AgentDatabaseHealthCheck(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var connectionString = _configuration
            .GetConnectionString("AgentDatabase");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return HealthCheckResult.Unhealthy(
                "Connection string 'AgentDatabase' is missing.");
        }

        try
        {
            await using var connection =
                new SqliteConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1;";

            var result = await command.ExecuteScalarAsync(
                cancellationToken);

            return Convert.ToInt32(result) == 1
                ? HealthCheckResult.Healthy(
                    "Agent SQLite database is reachable.")
                : HealthCheckResult.Unhealthy(
                    "Agent SQLite database returned an unexpected result.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Agent SQLite database is not reachable.",
                exception);
        }
    }
}
