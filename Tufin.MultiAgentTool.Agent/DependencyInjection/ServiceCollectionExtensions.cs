using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tufin.MultiAgentTool.Agent.Configuration;
using Tufin.MultiAgentTool.Agent.Orchestration;
using Tufin.MultiAgentTool.Agent.Serialization;
using Tufin.MultiAgentTool.Agent.Tools;
using Tufin.MultiAgentTool.Application.Agents;

namespace Tufin.MultiAgentTool.Agent.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentOrchestration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<AgentOptions>()
            .Bind(
                configuration.GetSection(
                    AgentOptions.SectionName))
            .Validate(
                options => options.MaxSteps > 0,
                "Agent MaxSteps must be greater than zero.")
            .Validate(
                options => options.MaxToolCallsPerStep > 0,
                "Agent MaxToolCallsPerStep must be greater than zero.")
            .Validate(
                options =>
                    options.Temperature is >= 0 and <= 2,
                "Agent Temperature must be between 0 and 2.")
            .ValidateOnStart();

        services.AddScoped<IAgentToolRegistry, AgentToolRegistry>();
        services.AddSingleton<AgentPromptBuilder>();
        services.AddSingleton<AgentDecisionSummaryFactory>();
        services.AddSingleton<AgentJsonSerializer>();

        services.AddScoped<IAgentRunner, AgentRunner>();

        return services;
    }
}