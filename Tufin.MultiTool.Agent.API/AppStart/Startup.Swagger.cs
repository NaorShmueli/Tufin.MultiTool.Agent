using System.Reflection;
using Microsoft.OpenApi;

namespace Tufin.MultiTool.Agent.API.AppStart;

public partial class Startup
{
    private const string contactEmail = "support@example.com";
    private const string ApiName = "Tufin.MultiTool.Agent.API";
    private const string ApiDescription = @"A production-grade, multi-step AI agent framework featuring fully observable reasoning chains, structured tool-calling, and native execution logging.";
    private const string ApiVersion = "1.0.0";

    private void ConfigureSwagger(IServiceCollection services)
    {
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1",
                new OpenApiInfo
                {
                    Title = ApiName, Version = ApiVersion, Description = ApiDescription,
                    Contact = new OpenApiContact { Name = ApiName }
                });
            c.IncludeXmlComments(xmlPath);
            //c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //{
            //    In = ParameterLocation.Header,
            //    Name = "Authorization",
            //    Type = SecuritySchemeType.Http,
            //    BearerFormat = "JWT",
            //    Scheme = JwtBearerDefaults.AuthenticationScheme
            //});

            //c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //{
            //    {
            //        new OpenApiSecurityScheme
            //        {
            //            Reference = new OpenApiReference
            //            {
            //                Type = ReferenceType.SecurityScheme,
            //                Id = JwtBearerDefaults.AuthenticationScheme
            //            }
            //        },
            //        new string[] { }
            //    }
            //});
        });
    }

    private void EnableSwagger(IApplicationBuilder app)
    {
        app.UseSwagger(c =>
        {
            c.RouteTemplate = "api/swagger/{documentName}/swagger.json"; // Add api prefix
            c.PreSerializeFilters.Add((swaggerDoc, request) =>
            {
                const string prefixHeader = "X-Forwarded-Prefix";
                if (!request.Headers.ContainsKey(prefixHeader))
                {
                    return;
                }
                var serverUrl = request.Headers[prefixHeader];
                swaggerDoc.Servers = new List<OpenApiServer>
                {
                    new() { Url = serverUrl }
                };
            });
        });
        app.UseSwaggerUI(c =>
        {
            c.RoutePrefix = "api/swagger"; // Change to api/swagger
            c.SwaggerEndpoint("/api/swagger/v1/swagger.json", ApiName); // Update endpoint
        });
    }
}