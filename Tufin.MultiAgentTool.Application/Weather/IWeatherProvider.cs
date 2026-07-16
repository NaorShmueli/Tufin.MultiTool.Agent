namespace Tufin.MultiAgentTool.Application.Weather;

/// <summary>
/// Retrieves current weather from an external weather provider.
/// </summary>
public interface IWeatherProvider
{
    Task<WeatherProviderResult> GetCurrentAsync(
        string city,
        string? countryCode,
        CancellationToken cancellationToken);
}