using Tufin.MultiAgentTool.Domain.Metrics;
using Tufin.MultiAgentTool.Domain.Tasks;
using Tufin.MultiAgentTool.Domain.Tracing;

namespace Tufin.MultiAgentTool.UnitTests.Domain;

public sealed class AgentTaskTests
{
    [Fact]
    public void Complete_ShouldStoreFinalStateAndAccumulateTokenUsage()
    {
        // Arrange
        var createdAt =
            new DateTimeOffset(2026, 7, 14, 10, 0, 0, TimeSpan.Zero);

        var task = AgentTask.Create(
            "What is the weather in London?",
            "llama3.2:3b",
            createdAt);

        task.Start(createdAt.AddSeconds(1));

        // Act
        task.RecordTrace(
            1,
            TraceEventType.ModelDecision,
            createdAt.AddSeconds(2),
            "Current weather data is required.",
            tokenUsage: new TokenUsage(
                120,
                25));

        task.Complete(
            "The current temperature in London is 20°C.",
            createdAt.AddSeconds(3));

        // Assert
        Assert.Equal(
            AgentTaskStatus.Completed,
            task.Status);

        Assert.Equal(
            "The current temperature in London is 20°C.",
            task.FinalAnswer);

        Assert.Equal(
            2_000,
            task.TotalLatencyMs);

        Assert.Equal(
            120,
            task.TokenUsage.PromptTokens);

        Assert.Equal(
            25,
            task.TokenUsage.CompletionTokens);

        Assert.Equal(
            145,
            task.TokenUsage.TotalTokens);

        Assert.Collection(
            task.TraceEvents,
            started =>
                Assert.Equal(
                    TraceEventType.TaskStarted,
                    started.EventType),
            decision =>
                Assert.Equal(
                    TraceEventType.ModelDecision,
                    decision.EventType),
            finalAnswer =>
                Assert.Equal(
                    TraceEventType.FinalAnswer,
                    finalAnswer.EventType),
            completed =>
                Assert.Equal(
                    TraceEventType.TaskCompleted,
                    completed.EventType));
    }

    [Fact]
    public void Complete_WhenTaskWasNotStarted_ShouldThrow()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var task = AgentTask.Create(
            "Calculate 2 + 2",
            "llama3.2:3b",
            now);

        // Act
        var action = () => task.Complete(
            "4",
            now.AddSeconds(1));

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void RecordTrace_ShouldAssignIncreasingSequenceNumbers()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var task = AgentTask.Create(
            "Convert 10 kilometers to miles.",
            "llama3.2:3b",
            now);

        task.Start(now);

        // Act
        task.RecordTrace(
            1,
            TraceEventType.ModelDecision,
            now.AddMilliseconds(10));

        task.RecordTrace(
            1,
            TraceEventType.ToolCall,
            now.AddMilliseconds(20),
            toolName: "unit_converter");

        task.RecordTrace(
            1,
            TraceEventType.ToolResult,
            now.AddMilliseconds(30),
            toolName: "unit_converter");

        // Assert
        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            task.TraceEvents
                .Select(traceEvent => traceEvent.Sequence)
                .ToArray());
    }
}