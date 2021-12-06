using Application;
using Application.Interfaces.Services;
using DocManager.Api.Middleware;
using DocManager.Api.Services;
using Infrastructure.Identity;
using Infrastructure.Persistence;
using Infrastructure.Shared;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace DocManagerTest.AplicationLayerTest
{
    public class BaseTest
    {
        public IMediator Mediator { get; set; }
        public IServiceCollection ServiceCollection { get; set; }
        public BaseTest()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Test.json")
            .Build();

            Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(config)
            .CreateLogger();


            ServiceCollection = new ServiceCollection();
            ServiceCollection.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(dispose: true));

            ServiceCollection.AddApplicationLayer();
            ServiceCollection.AddShareInfraestructureLayer(config);
            ServiceCollection.AddIdentityInfrastructureLayer(config);
            ServiceCollection.AddPersistenceInfrastructureLayer(config);
            ServiceCollection.AddTransient<ExceptionHandlingMiddleware>();

            ServiceProvider serviceProvider = ServiceCollection.BuildServiceProvider();

            Mediator = serviceProvider.GetService<IMediator>();
        }
    }
}