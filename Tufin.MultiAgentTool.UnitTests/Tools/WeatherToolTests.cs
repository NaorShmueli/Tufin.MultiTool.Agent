using System.Text.Json;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Application.Weather;
using Tufin.MultiAgentTool.Tools.Weather;

namespace Tufin.MultiAgentTool.UnitTests.Tools;

public sealed class WeatherToolTests
{
    [Fact]
    public async Task ExecuteAsync_ShouldReturnStructuredWeather()
    {
        // Arrange
        var weather = new CurrentWeatherData(
            RequestedCity: "London",
            ResolvedCity: "London",
            AdministrativeArea: "England",
            Country: "United Kingdom",
            CountryCode: "GB",
            Latitude: 51.5085,
            Longitude: -0.1257,
            WeatherTimeUtc:
                new DateTimeOffset(
                    2026,
                    7,
                    16,
                    10,
                    0,
                    0,
                    TimeSpan.Zero),
            TemperatureCelsius: 20,
            ApparentTemperatureCelsius: 19.5,
            RelativeHumidityPercent: 60,
            WindSpeedKmh: 12,
            WeatherCode: 2,
            Condition: "partly cloudy",
            IsDay: true,
            Source: "Open-Meteo");

        var provider = new StubWeatherProvider(
            WeatherProviderResult.Success(weather));

        var tool = new WeatherTool(provider);

        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                city = "London",
                countryCode = "GB"
            });

        // Act
        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Output);

        var output = result.Output.Value;

        Assert.Equal(
            "London",
            output
                .GetProperty("location")
                .GetProperty("resolvedCity")
                .GetString());

        Assert.Equal(
            20,
            output
                .GetProperty("current")
                .GetProperty("temperatureCelsius")
                .GetDouble());

        Assert.Equal(
            "partly cloudy",
            output
                .GetProperty("current")
                .GetProperty("condition")
                .GetString());

        Assert.Equal("London", provider.LastCity);
        Assert.Equal("GB", provider.LastCountryCode);
    }

    [Fact]
    public async Task ExecuteAsync_WhenCityIsMissing_ShouldFail()
    {
        // Arrange
        var provider = new StubWeatherProvider(
            WeatherProviderResult.Failure(
                "unexpected",
                "Provider should not have been called."));

        var tool = new WeatherTool(provider);

        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                countryCode = "GB"
            });

        // Act
        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "invalid_arguments",
            result.ErrorCode);

        Assert.Equal(0, provider.CallCount);
    }

    [Fact]
    public async Task ExecuteAsync_WhenLocationIsNotFound_ShouldReturnFailure()
    {
        // Arrange
        var provider = new StubWeatherProvider(
            WeatherProviderResult.Failure(
                errorCode: "location_not_found",
                errorMessage:
                    "No matching location was found."));

        var tool = new WeatherTool(provider);

        var arguments = JsonSerializer.SerializeToElement(
            new
            {
                city = "UnknownCity"
            });

        // Act
        var result = await tool.ExecuteAsync(
            arguments,
            new AgentToolExecutionContext(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(
            "location_not_found",
            result.ErrorCode);
    }

    private sealed class StubWeatherProvider
        : IWeatherProvider
    {
        private readonly WeatherProviderResult _result;

        public StubWeatherProvider(
            WeatherProviderResult result)
        {
            _result = result;
        }

        public int CallCount { get; private set; }

        public string? LastCity { get; private set; }

        public string? LastCountryCode { get; private set; }

        public Task<WeatherProviderResult> GetCurrentAsync(
            string city,
            string? countryCode,
            CancellationToken cancellationToken)
        {
            CallCount++;
            LastCity = city;
            LastCountryCode = countryCode;

            return Task.FromResult(_result);
        }
    }
}