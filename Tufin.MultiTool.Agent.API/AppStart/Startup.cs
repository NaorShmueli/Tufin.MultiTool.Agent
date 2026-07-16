using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Tufin.MultiAgentTool.Infrastructure.DependencyInjection;
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
        services.AddInfrastructureServices(_configuration);
        services.AddAgentTools();

        //services.AddMemoryCache();
        services.AddCors(options =>
        {
            //options.AddPolicy("AllowAll", builder =>
            //{
            //    builder.WithOrigins("https://domforgeai.com") // must include scheme + port
            //        .AllowCredentials()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //});
            //options.AddPolicy("AllowLocal", builder =>
            //{
            //    builder.WithOrigins("localhost") // must include scheme + port
            //        .AllowCredentials()
            //        .AllowAnyMethod()
            //        .AllowAnyHeader();
            //});
        });
        //services.AddRateLimiter(options => {
        //    options.AddFixedWindowLimiter("myPolicy", opt => {
        //        opt.PermitLimit = 1;
        //        opt.Window = TimeSpan.FromHours(1);
        //    });
        //});


        // Add SignalR (for NotificationHub)
        //services.AddSignalR();

        //services.ConfigureAuthentication(_configuration);
        //services.ConfigureAuthorization();
        services.AddControllers().AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.Converters.Add(new StringEnumConverter());
            options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        });




    }


    // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        app.Use(async (context, next) =>
        {
            var path = context.Request.Path.Value?.ToLower();
            var hasForwardedProto = context.Request.Headers.ContainsKey("X-Forwarded-Proto");
            var forwardedProto = context.Request.Headers["X-Forwarded-Proto"].FirstOrDefault();

            // Don't modify scheme for health check endpoints
            if (path?.StartsWith("/health") == true)
            {
                // Keep original scheme for health checks
                await next();
                return;
            }

            // For external traffic with forwarded proto header, use HTTPS
            if (hasForwardedProto && forwardedProto == "https")
            {
                context.Request.Scheme = "https";
            }

            await next();
        });

        //app.UseForwardedHeaders();
        app.UseRouting();
        app.UseCors("AllowAll");
        //app.UseRateLimiter();

        // ? Critical: Add this BEFORE Authentication
        app.UseCookiePolicy(new CookiePolicyOptions
        {
            MinimumSameSitePolicy = SameSiteMode.Lax,
            Secure = CookieSecurePolicy.SameAsRequest
        });
        //app.UseHttpsRedirection();
        //app.UseAuthentication();
        //app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();


            //// Liveness probe - just checks if app is alive
            //endpoints.MapHealthChecks("/health/live");

            //// Readiness probe - checks if app is ready to serve traffic
            //endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            //{
            //    Predicate = check => check.Tags.Contains("ready"),
            //    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            //});
        });


        EnableSwagger(app);
    }
}