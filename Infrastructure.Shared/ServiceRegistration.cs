using Application.Interfaces.Services;
using Domain.Settings;
using Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Shared
{
    public static class ServiceRegistration
    {
        public static void AddShareInfraestructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MailServiceSettings>(configuration.GetSection("MailServiceSettings"));
            services.AddTransient<IDateTimeService, DateTimeService>();
            services.AddTransient<IEmailService, EmailService>();
        }
    }
}