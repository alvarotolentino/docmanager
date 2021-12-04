using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence.Repositories;
using Application.DTOs.Account;
using Microsoft.AspNetCore.Identity;
using Domain.Settings;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;
using Application.Common;
using Utf8Json;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System.Data.Common;
using Domain.Entities;
using Infrastructure.Persistence.Database;

namespace Infrastructure.Persistence
{
    public enum DatabaseType
    {
        Metadata,
        Data
    }
    public delegate DbConnection DatabaseResolver(DatabaseType databaseType);
    public static class ServiceRegistration
    {


        public static void AddPersistenceInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {

            services.AddSingleton<DatabaseConnections>(provider =>
            {
                return new DatabaseConnections(
                metadataConnection: new NpgsqlConnection(configuration.GetConnectionString("DocumentMetadataConnection")),
                dataConnection: new NpgsqlConnection(configuration.GetConnectionString("DocumentDataConnection")));
            });

            services.AddTransient<IDocumentRepositoryAsync, DocumentRepositoryAsync>();
            services.AddTransient<IAccountRepositoryAsync, AccountRepositoryAsync>();
            services.AddTransient<IGroupRepositoryAsync, GroupRepositoryAsync>();

        }
    }
}