namespace Tufin.MultiAgentTool.Application.Weather;

/// <summary>
/// Provider-independent representation of current weather data.
///
/// The Agent and WeatherTool do not need to know whether this data
/// originated from Open-Meteo or another provider.
/// </summary>
public sealed record CurrentWeatherData(
    string RequestedCity,
    string ResolvedCity,
    string? AdministrativeArea,
    string? Country,
    string? CountryCode,
    double Latitude,
    double Longitude,
    DateTimeOffset WeatherTimeUtc,
    double TemperatureCelsius,
    double ApparentTemperatureCelsius,
    int RelativeHumidityPercent,
    double WindSpeedKmh,
    int WeatherCode,
    string Condition,
    bool IsDay,
    string Source);