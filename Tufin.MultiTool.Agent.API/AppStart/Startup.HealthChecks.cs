namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    private void ConfigureHealthChecks(IServiceCollection services)
    {
        //services.AddHealthChecks()
        //    .AddSqlServer(
        //        _configuration["SQL_CONNECTION"],
        //        name: "SQL Server",
        //        failureStatus: HealthStatus.Degraded,
        //        tags: new[] { "db", "sql", "ready" },
        //        timeout: TimeSpan.FromSeconds(3))
        //    .AddRedis(
        //        _configuration["RedisConnection"],
        //        "Redis Cache",
        //        HealthStatus.Degraded, // App can work without cache
        //        new[] { "cache", "redis", "ready" },
        //        TimeSpan.FromSeconds(2));
        //.AddCheck<KafkaHealthCheck>(
        //    "kafka",
        //    tags: new[] { "ready" });
    }
}