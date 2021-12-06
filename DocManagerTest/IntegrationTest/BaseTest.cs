using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace DocManagerTest.IntegrationTest
{
    public class BaseTest
    {
        public System.Net.Http.HttpClient Client { get; set; }
        public BaseTest()
        {
            var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration(p =>
            {
                p.AddJsonFile("appsettings.Test.json").Build();
            })
            .UseSerilog((HostBuilderContext context, LoggerConfiguration loggerConfiguration) =>
                {
                    loggerConfiguration.ReadFrom.Configuration(context.Configuration);
                })
            .ConfigureWebHost(webHost =>
            {
                webHost.UseTestServer();
                webHost.UseStartup<DocManager.Api.Startup>();
            })
            .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(LogLevel.Debug);
                });

            var host = hostBuilder.Start();
            Client = host.GetTestClient();
            Client.BaseAddress = new System.Uri("https://localhost:5001");
        }
    }
}