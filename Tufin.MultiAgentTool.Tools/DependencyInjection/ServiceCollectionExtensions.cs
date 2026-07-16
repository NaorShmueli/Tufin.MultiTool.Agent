using Microsoft.Extensions.DependencyInjection;
using Tufin.MultiAgentTool.Application.Tools;
using Tufin.MultiAgentTool.Tools.Calculator;
using Tufin.MultiAgentTool.Tools.UnitConversion;
using Tufin.MultiAgentTool.Tools.Weather;

namespace Tufin.MultiAgentTool.Tools.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentTools(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<SafeMathExpressionEvaluator>();
        services.AddSingleton<UnitConversionService>();

        services.AddScoped<IAgentTool, CalculatorTool>();
        services.AddScoped<IAgentTool, UnitConverterTool>();
        services.AddScoped<IAgentTool, WeatherTool>();

        return services;
    }
}