using System.Text.Json;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Tools.Database;

namespace Tufin.MultiAgentTool.UnitTests.Tools;

public sealed class DatabaseQueryToolTests : IDisposable
{
    private readonly string _databasePath = Path.Combine(
        Path.GetTempPath(),
        $"catalog-{Guid.NewGuid():N}.db");

    [Fact]
    public async Task ExecuteAsync_ShouldReturnCatalogRows()
    {
        var tool = await CreateToolAsync();
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                query = "SELECT name, price FROM products WHERE name = 'iPhone 17'"
            });

        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var output = result.Output!.Value;
        Assert.Equal(1, output.GetProperty("rowCount").GetInt32());
        Assert.Contains(
            "iPhone 17",
            output.GetRawText());
        Assert.Contains(
            "1199",
            output.GetRawText());
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryIsNotSelect_ShouldRejectIt()
    {
        var tool = await CreateToolAsync();
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                query = "DELETE FROM products"
            });

        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("unsafe_query", result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenQueryContainsMultipleStatements_ShouldRejectIt()
    {
        var tool = await CreateToolAsync();
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                query = "SELECT name FROM products; SELECT name FROM orders"
            });

        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("unsafe_query", result.ErrorCode);
    }

    public void Dispose()
    {
        TryDelete(_databasePath);
        TryDelete(_databasePath + "-shm");
        TryDelete(_databasePath + "-wal");
    }

    private async Task<DatabaseQueryTool> CreateToolAsync()
    {
        var options = Options.Create(
            new CatalogDatabaseOptions
            {
                ConnectionString = $"Data Source={_databasePath}"
            });

        var initializer = new CatalogDatabaseInitializer(options);
        await initializer.InitializeAsync();

        return new DatabaseQueryTool(options);
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
