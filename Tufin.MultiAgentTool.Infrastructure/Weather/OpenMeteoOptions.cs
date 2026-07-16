namespace Tufin.MultiAgentTool.Infrastructure.Weather;

public sealed class OpenMeteoOptions
{
    public const string SectionName = "OpenMeteo";

    public string GeocodingBaseUrl { get; set; } =
        "https://geocoding-api.open-meteo.com";

    public string ForecastBaseUrl { get; set; } =
        "https://api.open-meteo.com";

    public int TimeoutSeconds { get; set; } = 10;

    public void Validate()
    {
        if (!Uri.TryCreate(
                GeocodingBaseUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "OpenMeteo GeocodingBaseUrl must be an absolute URL.");
        }

        if (!Uri.TryCreate(
                ForecastBaseUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "OpenMeteo ForecastBaseUrl must be an absolute URL.");
        }

        if (TimeoutSeconds is < 1 or > 60)
        {
            throw new InvalidOperationException(
                "OpenMeteo TimeoutSeconds must be between 1 and 60.");
        }
    }
}