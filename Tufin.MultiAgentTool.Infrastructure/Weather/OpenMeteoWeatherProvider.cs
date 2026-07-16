using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Application.Weather;

namespace Tufin.MultiAgentTool.Infrastructure.Weather;

public sealed class OpenMeteoWeatherProvider : IWeatherProvider
{
    private readonly HttpClient _httpClient;
    private readonly OpenMeteoOptions _options;
    private readonly ILogger<OpenMeteoWeatherProvider> _logger;

    public OpenMeteoWeatherProvider(
        HttpClient httpClient,
        IOptions<OpenMeteoOptions> options,
        ILogger<OpenMeteoWeatherProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _options.Validate();
    }

    public async Task<WeatherProviderResult> GetCurrentAsync(
        string city,
        string? countryCode,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(city))
        {
            return WeatherProviderResult.Failure(
                errorCode: "invalid_city",
                errorMessage: "City is required.");
        }

        try
        {
            var location = await ResolveLocationAsync(
                city.Trim(),
                countryCode,
                cancellationToken);

            if (location is null)
            {
                return WeatherProviderResult.Failure(
                    errorCode: "location_not_found",
                    errorMessage:
                        $"No location matching '{city.Trim()}' was found.");
            }

            var current = await GetCurrentWeatherAsync(
                location.Latitude,
                location.Longitude,
                cancellationToken);

            if (current is null)
            {
                return WeatherProviderResult.Failure(
                    errorCode: "invalid_provider_response",
                    errorMessage:
                        "The weather provider returned no current weather data.");
            }

            var weather = new CurrentWeatherData(
                RequestedCity: city.Trim(),
                ResolvedCity: location.Name,
                AdministrativeArea: location.AdministrativeArea,
                Country: location.Country,
                CountryCode: location.CountryCode,
                Latitude: location.Latitude,
                Longitude: location.Longitude,
                WeatherTimeUtc:
                    DateTimeOffset.FromUnixTimeSeconds(
                        current.TimeUnixSeconds),
                TemperatureCelsius:
                    current.TemperatureCelsius,
                ApparentTemperatureCelsius:
                    current.ApparentTemperatureCelsius,
                RelativeHumidityPercent:
                    current.RelativeHumidityPercent,
                WindSpeedKmh:
                    current.WindSpeedKmh,
                WeatherCode:
                    current.WeatherCode,
                Condition:
                    OpenMeteoWeatherCodeMapper.ToDescription(
                        current.WeatherCode),
                IsDay:
                    current.IsDay == 1,
                Source:
                    "Open-Meteo");

            return WeatherProviderResult.Success(weather);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(
                exception,
                "Open-Meteo request timed out for city {City}.",
                city);

            return WeatherProviderResult.Failure(
                errorCode: "weather_provider_timeout",
                errorMessage:
                    "The weather provider did not respond within the configured timeout.");
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(
                exception,
                "Open-Meteo HTTP request failed for city {City}.",
                city);

            return WeatherProviderResult.Failure(
                errorCode: "weather_provider_unavailable",
                errorMessage:
                    "The weather provider is currently unavailable.");
        }
        catch (JsonException exception)
        {
            _logger.LogWarning(
                exception,
                "Open-Meteo returned invalid JSON for city {City}.",
                city);

            return WeatherProviderResult.Failure(
                errorCode: "invalid_provider_response",
                errorMessage:
                    "The weather provider returned an invalid response.");
        }
    }

    private async Task<OpenMeteoLocationDto?> ResolveLocationAsync(
        string city,
        string? countryCode,
        CancellationToken cancellationToken)
    {
        var requestUri =
            $"{_options.GeocodingBaseUrl.TrimEnd('/')}/v1/search" +
            $"?name={Uri.EscapeDataString(city)}" +
            "&count=1" +
            "&language=en" +
            "&format=json";

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            requestUri +=
                $"&countryCode={Uri.EscapeDataString(
                    countryCode.Trim().ToUpperInvariant())}";
        }

        using var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload =
            await response.Content
                .ReadFromJsonAsync<OpenMeteoGeocodingResponse>(
                    cancellationToken: cancellationToken);

        return payload?.Results?.FirstOrDefault();
    }

    private async Task<OpenMeteoCurrentWeatherDto?>
        GetCurrentWeatherAsync(
            double latitude,
            double longitude,
            CancellationToken cancellationToken)
    {
        var latitudeText =
            latitude.ToString(CultureInfo.InvariantCulture);

        var longitudeText =
            longitude.ToString(CultureInfo.InvariantCulture);

        var currentVariables =
            "temperature_2m," +
            "apparent_temperature," +
            "relative_humidity_2m," +
            "weather_code," +
            "wind_speed_10m," +
            "is_day";

        var requestUri =
            $"{_options.ForecastBaseUrl.TrimEnd('/')}/v1/forecast" +
            $"?latitude={latitudeText}" +
            $"&longitude={longitudeText}" +
            $"&current={currentVariables}" +
            "&temperature_unit=celsius" +
            "&wind_speed_unit=kmh" +
            "&timeformat=unixtime" +
            "&timezone=UTC";

        using var response = await _httpClient.GetAsync(
            requestUri,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        var payload =
            await response.Content
                .ReadFromJsonAsync<OpenMeteoForecastResponse>(
                    cancellationToken: cancellationToken);

        return payload?.Current;
    }
}