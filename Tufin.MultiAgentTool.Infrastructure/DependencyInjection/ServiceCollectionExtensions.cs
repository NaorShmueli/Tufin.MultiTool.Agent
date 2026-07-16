using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Tufin.MultiAgentTool.Application.Weather;
using Tufin.MultiAgentTool.Infrastructure.Weather;

namespace Tufin.MultiAgentTool.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<OpenMeteoOptions>()
            .Bind(
                configuration.GetSection(
                    OpenMeteoOptions.SectionName))
            .Validate(
                options =>
                    Uri.TryCreate(
                        options.GeocodingBaseUrl,
                        UriKind.Absolute,
                        out _),
                "OpenMeteo GeocodingBaseUrl must be an absolute URL.")
            .Validate(
                options =>
                    Uri.TryCreate(
                        options.ForecastBaseUrl,
                        UriKind.Absolute,
                        out _),
                "OpenMeteo ForecastBaseUrl must be an absolute URL.")
            .Validate(
                options =>
                    options.TimeoutSeconds is >= 1 and <= 60,
                "OpenMeteo TimeoutSeconds must be between 1 and 60.")
            .ValidateOnStart();

        services.AddHttpClient<
            IWeatherProvider,
            OpenMeteoWeatherProvider>(
            (serviceProvider, httpClient) =>
            {
                var options = serviceProvider
                    .GetRequiredService<
                        IOptions<OpenMeteoOptions>>()
                    .Value;

                httpClient.Timeout =
                    TimeSpan.FromSeconds(
                        options.TimeoutSeconds);

                httpClient.DefaultRequestHeaders
                    .UserAgent
                    .ParseAdd(
                        "Tufin-MultiAgentTool/1.0");
            });

        return services;
    }
}