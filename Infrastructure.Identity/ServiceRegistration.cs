using Domain.Entities;
using Domain.Settings;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Application.Common;
using Microsoft.AspNetCore.Http;
using Utf8Json;
using Infrastructure.Persistence.Repositories;

namespace Infrastructure.Identity
{
    public static class ServiceRegistration
    {
        private const string INTERNALERROR = "Server Error";
        private const string UNAUTHORIZED = "You are not Authorized";
        private const string ACCESSDENIED = "You are not authorized to access this resource";

        public static void AddIdentityInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            
            services.AddTransient<IUserStore<User>, AccountRepositoryAsync>();
            services.AddTransient<IRoleStore<Role>, RoleRepositoryAsync>();
            services.AddIdentity<User, Role>()
            // .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            services.Configure<JWTokenSettings>(configuration.GetSection("JWTokenSettings"));
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(o =>
            {
                o.RequireHttpsMetadata = false;
                o.SaveToken = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = configuration["JWTokenSettings:Issuer"],
                    ValidAudience = configuration["JWTokenSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWTokenSettings:Key"]))
                };
                o.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = (context) =>
                    {
                        context.NoResult();
                        context.Response.StatusCode = 500;
                        context.Response.ContentType = "application/json";
                        var message = new Response<string>(message: INTERNALERROR, succeeded: false);
                        return context.Response.WriteAsync(JsonSerializer.ToJsonString(message));
                    },
                    OnChallenge = (context) =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = 401;
                        context.Response.ContentType = "application/json";
                        var message = new Response<string>(message: UNAUTHORIZED, succeeded: false);
                        return context.Response.WriteAsync(JsonSerializer.ToJsonString(message));
                    },
                    OnForbidden = (context) =>
                    {
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/json";
                        var message = new Response<string>(message: ACCESSDENIED, succeeded: false);
                        return context.Response.WriteAsync(JsonSerializer.ToJsonString(message));

                    }
                };

            });

        }
    }

}
