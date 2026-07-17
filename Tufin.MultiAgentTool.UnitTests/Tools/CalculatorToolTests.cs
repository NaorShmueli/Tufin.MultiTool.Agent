using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Tools.Calculator;

namespace Tufin.MultiAgentTool.UnitTests.Tools;

public sealed class CalculatorToolTests
{
    private readonly CalculatorTool _tool =
        new(new SafeMathExpressionEvaluator());

    [Fact]
    public async Task ExecuteAsync_ShouldRespectOperatorPrecedence()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                expression = "2 + 3 * 4"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);

        var numericResult = result.Output.Value
            .GetProperty("result")
            .GetDouble();

        Assert.Equal(14, numericResult);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldEvaluatePercentageExpression()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                expression = "68 * 0.15"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var numericResult = result.Output!.Value
            .GetProperty("result")
            .GetDouble();

        Assert.Equal(
            10.2,
            numericResult,
            10);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDividingByZero_ShouldFailSafely()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                expression = "10 / 0"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "invalid_expression",
            result.ErrorCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCodeLikeInputIsProvided_ShouldRejectIt()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                expression =
                    "System.IO.File.Delete(\"important.txt\")"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "invalid_expression",
            result.ErrorCode);
    }
}