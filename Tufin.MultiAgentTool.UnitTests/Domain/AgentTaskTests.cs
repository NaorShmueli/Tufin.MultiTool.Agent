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
            input: "What is the weather in London?",
            model: "llama3.2:3b",
            createdAt: createdAt);

        task.Start(createdAt.AddSeconds(1));

        // Act
        task.RecordTrace(
            stepNumber: 1,
            eventType: TraceEventType.ModelDecision,
            occurredAt: createdAt.AddSeconds(2),
            decisionSummary: "Current weather data is required.",
            tokenUsage: new TokenUsage(
                promptTokens: 120,
                completionTokens: 25));

        task.Complete(
            finalAnswer: "The current temperature in London is 20°C.",
            completedAt: createdAt.AddSeconds(3));

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
            input: "Calculate 2 + 2",
            model: "llama3.2:3b",
            createdAt: now);

        // Act
        var action = () => task.Complete(
            finalAnswer: "4",
            completedAt: now.AddSeconds(1));

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void RecordTrace_ShouldAssignIncreasingSequenceNumbers()
    {
        // Arrange
        var now = DateTimeOffset.UtcNow;

        var task = AgentTask.Create(
            input: "Convert 10 kilometers to miles.",
            model: "llama3.2:3b",
            createdAt: now);

        task.Start(now);

        // Act
        task.RecordTrace(
            stepNumber: 1,
            eventType: TraceEventType.ModelDecision,
            occurredAt: now.AddMilliseconds(10));

        task.RecordTrace(
            stepNumber: 1,
            eventType: TraceEventType.ToolCall,
            occurredAt: now.AddMilliseconds(20),
            toolName: "unit_converter");

        task.RecordTrace(
            stepNumber: 1,
            eventType: TraceEventType.ToolResult,
            occurredAt: now.AddMilliseconds(30),
            toolName: "unit_converter");

        // Assert
        Assert.Equal(
            new[] { 1, 2, 3, 4 },
            task.TraceEvents
                .Select(traceEvent => traceEvent.Sequence)
                .ToArray());
    }
}