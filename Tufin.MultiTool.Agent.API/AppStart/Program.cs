using System.Diagnostics;

namespace Tufin.MultiTool.Agent.API.AppStart;

public class Program
{
    public static void Main(string[] args)
    {
        Activity.DefaultIdFormat = ActivityIdFormat.W3C;
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args)
    {
        return Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                //webBuilder.UseUrls("http://0.0.0.0:80");

                webBuilder.UseStartup<Startup>();
            });
    }
}