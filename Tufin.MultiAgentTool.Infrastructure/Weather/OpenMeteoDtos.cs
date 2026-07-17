using System.Text.Json.Serialization;

namespace Tufin.MultiAgentTool.Infrastructure.Weather;

internal sealed class OpenMeteoGeocodingResponse
{
    [JsonPropertyName("results")] public List<OpenMeteoLocationDto>? Results { get; init; }
}

internal sealed class OpenMeteoLocationDto
{
    [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;

    [JsonPropertyName("latitude")] public double Latitude { get; init; }

    [JsonPropertyName("longitude")] public double Longitude { get; init; }

    [JsonPropertyName("country")] public string? Country { get; init; }

    [JsonPropertyName("country_code")] public string? CountryCode { get; init; }

    [JsonPropertyName("admin1")] public string? AdministrativeArea { get; init; }

    [JsonPropertyName("timezone")] public string? TimeZone { get; init; }
}

internal sealed class OpenMeteoForecastResponse
{
    [JsonPropertyName("current")] public OpenMeteoCurrentWeatherDto? Current { get; init; }
}

internal sealed class OpenMeteoCurrentWeatherDto
{
    [JsonPropertyName("time")] public long TimeUnixSeconds { get; init; }

    [JsonPropertyName("temperature_2m")] public double TemperatureCelsius { get; init; }

    [JsonPropertyName("apparent_temperature")]
    public double ApparentTemperatureCelsius { get; init; }

    [JsonPropertyName("relative_humidity_2m")]
    public int RelativeHumidityPercent { get; init; }

    [JsonPropertyName("wind_speed_10m")] public double WindSpeedKmh { get; init; }

    [JsonPropertyName("weather_code")] public int WeatherCode { get; init; }

    [JsonPropertyName("is_day")] public int IsDay { get; init; }
}