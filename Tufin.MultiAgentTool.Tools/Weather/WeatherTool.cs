using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Application.Weather;

namespace Tufin.MultiAgentTool.Tools.Weather;

public sealed class WeatherTool : IAgentTool
{
    private readonly IWeatherProvider _weatherProvider;

    public WeatherTool(IWeatherProvider weatherProvider)
    {
        _weatherProvider = weatherProvider;
    }

    public AgentToolDefinition Definition { get; } =
        new(
            name: "weather",
            description:
                """
                Fetches current weather conditions for a city using a live
                external weather provider.

                Use this tool whenever the user asks about current weather,
                current temperature, apparent temperature, humidity, wind,
                or current weather conditions.

                The result is returned in Celsius and km/h.

                When the user requests Fahrenheit or another unit, call this
                weather tool first and then pass its numeric result to the
                unit_converter tool.

                Do not invent current weather information.
                """,
            inputSchema: JsonSerializer.SerializeToElement(
                new
                {
                    type = "object",
                    properties = new
                    {
                        city = new
                        {
                            type = "string",
                            description =
                                "The city whose current weather is required.",
                            minLength = 1,
                            maxLength = 100
                        },
                        countryCode = new
                        {
                            type = "string",
                            description =
                                "Optional two-letter ISO country code used " +
                                "to disambiguate cities, for example GB, " +
                                "FR, US or IL.",
                            minLength = 2,
                            maxLength = 2
                        }
                    },
                    required = new[] { "city" },
                    additionalProperties = false
                }));

    public async Task<AgentToolExecutionResult> ExecuteAsync(
        JsonElement arguments,
        AgentToolExecutionContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!TryReadRequiredString(
                arguments,
                "city",
                out var city))
        {
            return AgentToolExecutionResult.Failure(
                errorCode: "invalid_arguments",
                errorMessage:
                    "A non-empty string property named 'city' is required.");
        }

        string? countryCode = null;

        if (arguments.TryGetProperty(
                "countryCode",
                out var countryCodeProperty))
        {
            if (countryCodeProperty.ValueKind !=
                JsonValueKind.String)
            {
                return AgentToolExecutionResult.Failure(
                    errorCode: "invalid_arguments",
                    errorMessage:
                        "'countryCode' must be a string.");
            }

            countryCode =
                countryCodeProperty.GetString()?.Trim();

            if (!string.IsNullOrWhiteSpace(countryCode) &&
                (countryCode.Length != 2 ||
                 !countryCode.All(char.IsLetter)))
            {
                return AgentToolExecutionResult.Failure(
                    errorCode: "invalid_arguments",
                    errorMessage:
                        "'countryCode' must contain exactly two letters.");
            }
        }

        var providerResult =
            await _weatherProvider.GetCurrentAsync(
                city!,
                countryCode,
                cancellationToken);

        if (!providerResult.IsSuccess ||
            providerResult.Weather is null)
        {
            return AgentToolExecutionResult.Failure(
                errorCode:
                    providerResult.ErrorCode ??
                    "weather_lookup_failed",
                errorMessage:
                    providerResult.ErrorMessage ??
                    "Current weather could not be retrieved.");
        }

        var weather = providerResult.Weather;

        var output = JsonSerializer.SerializeToElement(
            new
            {
                location = new
                {
                    requestedCity =
                        weather.RequestedCity,
                    resolvedCity =
                        weather.ResolvedCity,
                    administrativeArea =
                        weather.AdministrativeArea,
                    country =
                        weather.Country,
                    countryCode =
                        weather.CountryCode,
                    latitude =
                        weather.Latitude,
                    longitude =
                        weather.Longitude
                },
                current = new
                {
                    weatherTimeUtc =
                        weather.WeatherTimeUtc,
                    temperatureCelsius =
                        weather.TemperatureCelsius,
                    apparentTemperatureCelsius =
                        weather.ApparentTemperatureCelsius,
                    relativeHumidityPercent =
                        weather.RelativeHumidityPercent,
                    windSpeedKmh =
                        weather.WindSpeedKmh,
                    weatherCode =
                        weather.WeatherCode,
                    condition =
                        weather.Condition,
                    isDay =
                        weather.IsDay
                },
                source = weather.Source
            });

        return AgentToolExecutionResult.Success(output);
    }

    private static bool TryReadRequiredString(
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

        value = property.GetString()?.Trim();

        return !string.IsNullOrWhiteSpace(value);
    }
}