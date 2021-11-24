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
using Infrastructure.Shared;
using Application.Interfaces.Services;
using DocManager.Api.Services;
using DocManager.Api.Infrastructure.Formatter;
using Infrastructure.Identity;

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
            services.AddApplicationLayer();
            services.AddShareInfraestructureLayer(Configuration);
            services.AddIdentityInfrastructureLayer(Configuration);
            services.AddPersistenceInfrastructureLayer(Configuration);
            services
            .AddCustomMVC(Configuration)
            .AddCustomVersioning(Configuration)
            .AddSwagger(Configuration)
            .AddCustomHealthCheck(Configuration);
            services.AddControllers();
            services.AddTransient<ExceptionHandlingMiddleware>();
            services.AddScoped<IAuthenticatedUserService, AuthenticatedUserService>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app,
        IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<ExceptionHandlingMiddleware>();
            app.UseMiddleware<JwtMiddleware>();
            app.UseHttpsRedirection();
            app.UseSwagger();
            app.UseSwaggerUI(o =>
            {
                o.SwaggerEndpoint("/swagger/v1/swagger.json", "Document Manager API v1");
            });

            app.UseCors("CorsPolicy");
            app.UseRouting();
            app.UseAuthentication();
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

        public static IServiceCollection AddCustomMVC(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers();
            // .AddMvcOptions(options =>
            // {
            //     // options.OutputFormatters.Clear();
            //     options.OutputFormatters.Add(new Utf8JsonOutputFormatter(Utf8Json.Resolvers.StandardResolver.Default));
            //     // options.InputFormatters.Clear();
            //     options.InputFormatters.Add(new Utf8JsonInputFormatter());
            // });

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                builder => builder.SetIsOriginAllowed((host) => true)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());
            });
            return services;
        }
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
                o.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });
                o.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });
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
