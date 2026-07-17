using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tufin.MultiAgentTool.Persistence.Initialization;
using Tufin.MultiTool.Agent.API.AppStart;

namespace Tufin.MultiAgentTool.IntegrationTests;

public sealed class ApiSmokeTests
{
    [Fact]
    public async Task Health_WhenOllamaIsUnavailable_ShouldReturnServiceUnavailable()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(body);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        Assert.Equal(
            "Unhealthy",
            document.RootElement
                .GetProperty("status")
                .GetString());
        Assert.Contains(
            document.RootElement
                .GetProperty("checks")
                .EnumerateArray(),
            check => check.GetProperty("name").GetString() == "ollama");
    }

    [Fact]
    public async Task Tasks_ShouldReturnEmptyListForFreshDatabase()
    {
        await using var factory = new TestApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/tasks");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("[]", body);
    }

    private sealed class TestApiFactory
        : WebApplicationFactory<Program>
    {
        private readonly string _databasePath = Path.Combine(
            Path.GetTempPath(),
            $"agent-observability-{Guid.NewGuid():N}.db");

        protected override IHost CreateHost(
            IHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:AgentDatabase"] =
                            $"Data Source={_databasePath}",
                        ["Ollama:BaseUrl"] = "http://127.0.0.1:1",
                        ["Ollama:Model"] = "scripted-test-model"
                    });
            });

            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var initializer = scope.ServiceProvider
                .GetRequiredService<AgentDatabaseInitializer>();

            initializer.InitializeAsync().GetAwaiter().GetResult();

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            TryDelete(_databasePath);
            TryDelete(_databasePath + "-shm");
            TryDelete(_databasePath + "-wal");
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
}
