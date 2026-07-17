using System.Globalization;
using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;

namespace Tufin.MultiAgentTool.Tools.UnitConversion;

public sealed class UnitConverterTool : IAgentTool
{
    private readonly UnitConversionService _conversionService;

    public UnitConverterTool(
        UnitConversionService conversionService)
    {
        _conversionService = conversionService;
    }


    public AgentToolDefinition Definition { get; } =
        new(
            "unit_converter",
            @"
                Converts a numeric value between supported units.

                Supported categories:
                - temperature: celsius, fahrenheit, kelvin
                - length: meter, kilometer, centimeter, millimeter,
                  mile, yard, foot, inch
                - weight: kilogram, gram, pound, ounce

                Use this tool only when the numeric source value is already
                known. If current weather is required, call the weather tool
                first and then convert the returned temperature.

                Currency conversion is not supported because exchange rates
                are time-dependent and require a separate live data provider.
                ",
            JsonSerializer.SerializeToElement(
                new
                {
                    type = "object",
                    properties = new
                    {
                        value = new
                        {
                            type = "number",
                            description = "The numeric value to convert."
                        },
                        fromUnit = new
                        {
                            type = "string",
                            @enum = new[]
                            {
                                "celsius",
                                "fahrenheit",
                                "kelvin",
                                "meter",
                                "kilometer",
                                "centimeter",
                                "millimeter",
                                "mile",
                                "yard",
                                "foot",
                                "inch",
                                "kilogram",
                                "gram",
                                "pound",
                                "ounce"
                            }
                        },
                        toUnit = new
                        {
                            type = "string",
                            @enum = new[]
                            {
                                "celsius",
                                "fahrenheit",
                                "kelvin",
                                "meter",
                                "kilometer",
                                "centimeter",
                                "millimeter",
                                "mile",
                                "yard",
                                "foot",
                                "inch",
                                "kilogram",
                                "gram",
                                "pound",
                                "ounce"
                            }
                        }
                    },
                    required = new[]
                    {
                        "value",
                        "fromUnit",
                        "toUnit"
                    },
                    additionalProperties = false
                }));

    public Task<AgentToolExecutionResult> ExecuteAsync(
        JsonElement arguments,
        AgentToolExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryReadDouble(
                arguments,
                "value",
                out var value))
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_arguments",
                    "A numeric property named 'value' is required."));
        }

        if (!TryReadString(
                arguments,
                "fromUnit",
                out var fromUnit))
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_arguments",
                    "A string property named 'fromUnit' is required."));
        }

        if (!TryReadString(
                arguments,
                "toUnit",
                out var toUnit))
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "invalid_arguments",
                    "A string property named 'toUnit' is required."));
        }

        try
        {
            var result = _conversionService.Convert(
                value,
                fromUnit!,
                toUnit!);

            var output = JsonSerializer.SerializeToElement(
                new
                {
                    input = new
                    {
                        value = result.InputValue,
                        unit = result.FromUnit
                    },
                    output = new
                    {
                        value = result.OutputValue,
                        unit = result.ToUnit
                    },
                    category = result.Category
                });

            return Task.FromResult(
                AgentToolExecutionResult.Success(output));
        }
        catch (UnitConversionException exception)
        {
            return Task.FromResult(
                AgentToolExecutionResult.Failure(
                    "conversion_failed",
                    exception.Message));
        }
    }

    private static bool TryReadDouble(
        JsonElement arguments,
        string propertyName,
        out double value)
    {
        value = default;

        if (!arguments.TryGetProperty(
                propertyName,
                out var property))
        {
            return false;
        }

        if (property.ValueKind == JsonValueKind.Number)
        {
            return property.TryGetDouble(out value);
        }

        if (property.ValueKind == JsonValueKind.String)
        {
            return double.TryParse(
                property.GetString(),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out value);
        }

        return false;
    }

    private static bool TryReadString(
        JsonElement arguments,
        string propertyName,
        out string? value)
    {
        value = null;

        if (!arguments.TryGetProperty(
                propertyName,
                out var property) ||
            property.ValueKind != JsonValueKind.String)
        {
            return false;
        }

        value = property.GetString();

        return !string.IsNullOrWhiteSpace(value);
    }
}