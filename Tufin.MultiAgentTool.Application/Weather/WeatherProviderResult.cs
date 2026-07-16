namespace Tufin.MultiAgentTool.Application.Weather;

/// <summary>
/// Explicit provider result.
///
/// Expected provider problems are represented as structured failures
/// rather than exceptions that terminate the entire Agent loop.
/// </summary>
public sealed class WeatherProviderResult
{
    private WeatherProviderResult(
        bool isSuccess,
        CurrentWeatherData? weather,
        string? errorCode,
        string? errorMessage)
    {
        IsSuccess = isSuccess;
        Weather = weather;
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }

    public CurrentWeatherData? Weather { get; }

    public string? ErrorCode { get; }

    public string? ErrorMessage { get; }

    public static WeatherProviderResult Success(
        CurrentWeatherData weather)
    {
        ArgumentNullException.ThrowIfNull(weather);

        return new WeatherProviderResult(
            isSuccess: true,
            weather: weather,
            errorCode: null,
            errorMessage: null);
    }

    public static WeatherProviderResult Failure(
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorCode))
        {
            throw new ArgumentException(
                "Error code is required.",
                nameof(errorCode));
        }

        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException(
                "Error message is required.",
                nameof(errorMessage));
        }

        return new WeatherProviderResult(
            isSuccess: false,
            weather: null,
            errorCode: errorCode.Trim(),
            errorMessage: errorMessage.Trim());
    }
}