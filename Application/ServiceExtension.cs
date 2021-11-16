using System.Reflection;
using FluentValidation;
using MediatR;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Application.Behaviours;

namespace Application
{
    public static class ServiceExtension
    {
        public static void AddApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
            services.AddMediatR(Assembly.GetExecutingAssembly())
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehaviour<,>))
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidationBehavior<,>));
        }

    }
}