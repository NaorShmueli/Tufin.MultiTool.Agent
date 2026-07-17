using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Tools.UnitConversion;

namespace Tufin.MultiAgentTool.UnitTests.Tools;

public sealed class UnitConverterToolTests
{
    private readonly UnitConverterTool _tool =
        new(new UnitConversionService());

    [Fact]
    public async Task ExecuteAsync_ShouldConvertCelsiusToFahrenheit()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                value = 20,
                fromUnit = "celsius",
                toUnit = "fahrenheit"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var convertedValue = result.Output!.Value
            .GetProperty("output")
            .GetProperty("value")
            .GetDouble();

        Assert.Equal(
            68,
            convertedValue,
            10);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldConvertKilometersToMiles()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                value = 10,
                fromUnit = "kilometer",
                toUnit = "mile"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.True(result.IsSuccess);

        var convertedValue = result.Output!.Value
            .GetProperty("output")
            .GetProperty("value")
            .GetDouble();

        Assert.Equal(
            6.2137119224,
            convertedValue,
            8);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCategoriesDiffer_ShouldFail()
    {
        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                value = 10,
                fromUnit = "kilometer",
                toUnit = "kilogram"
            });

        var result = await _tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            "conversion_failed",
            result.ErrorCode);
    }
}