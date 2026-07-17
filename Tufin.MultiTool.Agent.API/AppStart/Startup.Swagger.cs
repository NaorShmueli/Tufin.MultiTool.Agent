using System.Reflection;
using Microsoft.OpenApi;

namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    private const string ApiName = "Tufin.MultiTool.Agent.API";

    private const string ApiDescription =
        @"A production-grade, multi-step AI agent framework featuring fully observable reasoning chains, structured tool-calling, and native execution logging.";

    private const string ApiVersion = "1.0.0";

    private void ConfigureSwagger(IServiceCollection services)
    {
        var xmlFile =
            $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

        var xmlPath =
            Path.Combine(AppContext.BaseDirectory, xmlFile);

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = ApiName,
                    Version = ApiVersion,
                    Description = ApiDescription,
                    Contact = new OpenApiContact
                    {
                        Name = ApiName
                    }
                });

            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }
        });
    }

    private void EnableSwagger(IApplicationBuilder app)
    {
        app.UseSwagger(c => { c.RouteTemplate = "swagger/{documentName}/swagger.json"; });

        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "swagger";
            c.SwaggerEndpoint(
                "/swagger/v1/swagger.json",
                ApiName);
        });
    }
}