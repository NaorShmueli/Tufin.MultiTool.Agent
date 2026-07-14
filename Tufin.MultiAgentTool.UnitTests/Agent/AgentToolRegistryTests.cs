using System.Text.Json;
using Tufin.MultiAgentTool.Agent.Tools;
using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.UnitTests.Agent;

public sealed class AgentToolRegistryTests
{
    [Fact]
    public void TryResolve_ShouldResolveToolCaseInsensitively()
    {
        // Arrange
        var tool = new StubTool("weather");

        var registry = new AgentToolRegistry(
            new[] { tool });

        // Act
        var found = registry.TryResolve(
            "WEATHER",
            out var resolved);

        // Assert
        Assert.True(found);
        Assert.Same(tool, resolved);
    }

    [Fact]
    public void Constructor_WhenNamesAreDuplicated_ShouldThrow()
    {
        // Arrange
        var tools = new IAgentTool[]
        {
            new StubTool("weather"),
            new StubTool("WEATHER")
        };

        // Act
        var action = () =>
            new AgentToolRegistry(tools);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    private sealed class StubTool : IAgentTool
    {
        public StubTool(string name)
        {
            Definition = new AgentToolDefinition(
                name,
                "Stub tool.",
                JsonSerializer.SerializeToElement(
                    new
                    {
                        type = "object",
                        properties = new { }
                    }));
        }

        public AgentToolDefinition Definition { get; }

        public Task<AgentToolExecutionResult> ExecuteAsync(
            JsonElement arguments,
            AgentToolExecutionContext context,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(
                AgentToolExecutionResult.Success(
                    JsonSerializer.SerializeToElement(
                        new { ok = true })));
        }
    }
}