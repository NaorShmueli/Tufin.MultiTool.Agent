using Microsoft.Extensions.DependencyInjection;
using Tufin.MultiAgentTool.Application.Tasks;

namespace Tufin.MultiAgentTool.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<
            ISubmitAgentTaskService,
            SubmitAgentTaskService>();

        services.AddScoped<
            IGetAgentTaskService,
            GetAgentTaskService>();

        return services;
    }
}