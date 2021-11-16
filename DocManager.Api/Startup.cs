using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Utf8Json;
using HealthChecks.NpgSql;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using DocManager.Api.Health;
using DocManager.Api.Middleware;
using FluentValidation;
using MediatR;
using Application;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Context;

namespace DocManager.Api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddPersistenceLayer(Configuration);
            services
            .AddCustomVersioning(Configuration)
            .AddSwagger(Configuration)
            .AddCustomHealthCheck(Configuration);
            services.AddApplicationLayer();
            services.AddTransient<ExceptionHandlingMiddleware>();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
        IWebHostEnvironment env, ApplicationDbContext db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            db.Database.EnsureCreated();

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseHttpsRedirection();

            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Manager API v1");
            });

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new HealthCheckOptions
                {
                    Predicate = _ => true,
                    ResponseWriter = async (context, report) =>
              {
                  context.Response.ContentType = "application/json";
                  var response = new HealthCheckResponse
                  {
                      Status = report.Status.ToString(),
                      HealthChecks = report.Entries.Select(x => new ItemHealthCheckResponse
                      {
                          Component = x.Key,
                          Status = x.Value.Status.ToString(),
                          Description = x.Value.Description
                      }),
                      HealthCheckDuration = report.TotalDuration
                  };
                  await context.Response.Body.WriteAsync(JsonSerializer.Serialize(response));
              }
                });
            });
        }
    }

    public static class CustomExtensionMethods
    {

        public static IServiceCollection AddSwagger(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSwaggerGen(o =>
            {
                o.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Document Manager API Service",
                    Description = "Clean Architecture for Document Manager Rest API Service"
                });
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                o.IncludeXmlComments(xmlPath);
            });

            return services;
        }
        public static IServiceCollection AddCustomVersioning(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddApiVersioning(o =>
            {
                o.ReportApiVersions = true;
                o.AssumeDefaultVersionWhenUnspecified = true;
                o.DefaultApiVersion = new ApiVersion(1, 0);
            });
            return services;
        }

        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();

            return services;
        }
    }
}
