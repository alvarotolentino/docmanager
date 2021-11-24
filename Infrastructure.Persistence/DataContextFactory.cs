
using System.IO;
using Infrastructure.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class DataContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var filePath = $"{Directory.GetCurrentDirectory()}/../DocManager.Api";
            var configuration = new ConfigurationBuilder()
            .SetBasePath(filePath)
            .AddJsonFile("appsettings.json")
            .Build();

            var connectionString = configuration
                  .GetConnectionString("DocumentManagerConnection");

            var builder = new DbContextOptionsBuilder<ApplicationDbContext>();
            builder.UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention();
            return new ApplicationDbContext(builder.Options, null, null, configuration);
        }
    }
}