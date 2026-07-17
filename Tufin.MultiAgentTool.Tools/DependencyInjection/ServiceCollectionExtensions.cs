using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Tools.Calculator;
using Tufin.MultiAgentTool.Tools.Database;
using Tufin.MultiAgentTool.Tools.UnitConversion;
using Tufin.MultiAgentTool.Tools.Weather;

namespace Tufin.MultiAgentTool.Tools.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentTools(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<CatalogDatabaseOptions>()
            .Bind(configuration.GetSection(
                CatalogDatabaseOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(
                        options.ConnectionString),
                "CatalogDatabase ConnectionString is required.")
            .ValidateOnStart();

        services.AddSingleton<SafeMathExpressionEvaluator>();
        services.AddSingleton<UnitConversionService>();
        services.AddScoped<CatalogDatabaseInitializer>();

        services.AddScoped<IAgentTool, CalculatorTool>();
        services.AddScoped<IAgentTool, UnitConverterTool>();
        services.AddScoped<IAgentTool, WeatherTool>();
        services.AddScoped<IAgentTool, DatabaseQueryTool>();

        return services;
    }
}
