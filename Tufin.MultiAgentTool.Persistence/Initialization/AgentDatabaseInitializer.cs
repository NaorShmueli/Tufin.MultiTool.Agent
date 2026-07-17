using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Tufin.MultiAgentTool.Persistence.Database;

namespace Tufin.MultiAgentTool.Persistence.Initialization;

public sealed class AgentDatabaseInitializer
{
    private readonly IConfiguration _configuration;
    private readonly AgentDbContext _dbContext;

    public AgentDatabaseInitializer(
        AgentDbContext dbContext,
        IConfiguration configuration)
    {
        _dbContext = dbContext;
        _configuration = configuration;
    }

    public async Task InitializeAsync(
        CancellationToken cancellationToken = default)
    {
        var connectionString =
            _configuration.GetConnectionString(
                "AgentDatabase");

        if (string.IsNullOrWhiteSpace(
                connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'AgentDatabase' is required.");
        }

        EnsureDatabaseDirectoryExists(
            connectionString);

        await _dbContext.Database.MigrateAsync(
            cancellationToken);
    }

    private static void EnsureDatabaseDirectoryExists(
        string connectionString)
    {
        var builder =
            new SqliteConnectionStringBuilder(
                connectionString);

        var dataSource = builder.DataSource;

        if (string.IsNullOrWhiteSpace(dataSource) ||
            dataSource.Equals(
                ":memory:",
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var fullPath =
            Path.GetFullPath(dataSource);

        var directory =
            Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}