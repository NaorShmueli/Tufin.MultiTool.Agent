using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tufin.MultiAgentTool.Agent.DependencyInjection;
using Tufin.MultiAgentTool.Application.DependencyInjection;
using Tufin.MultiAgentTool.Infrastructure.DependencyInjection;
using Tufin.MultiAgentTool.Persistence.DependencyInjection;
using Tufin.MultiAgentTool.Tools.DependencyInjection;

namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    //This method gets called by the runtime. Use this method to add services to the container.
    //Microsoft DI
    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureLogs(services);
        ConfigureSwagger(services);
        ConfigureHealthChecks(services);
        services.AddApplicationServices();
        services.AddInfrastructureServices(_configuration);
        services.AddAgentTools(_configuration);
        services.AddAgentPersistence(_configuration);
        services.AddAgentOrchestration(_configuration);
        //services.AddMemoryCache();
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            options.SerializerSettings.NullValueHandling =
                NullValueHandling.Ignore;
        });
    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            //app.UseHttpsRedirection();
        }

        app.UseRouting();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks(
                "/health",
                new HealthCheckOptions
                {
                    ResponseWriter = WriteHealthCheckResponse
                });
        });


        EnableSwagger(app);
    }
}
