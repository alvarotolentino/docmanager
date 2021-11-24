using Application.DTOs.Account;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence.Context
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<long>, long>
    {
        public IDateTimeService DateTimeService { get; private set; }
        public IAuthenticatedUserService AuthenticatedUserService { get; private set; }

        private IConfiguration configuration { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options,
        IDateTimeService dateTime, IAuthenticatedUserService authenticationUser, IConfiguration configuration)
        : base(options)
        {
            DateTimeService = dateTime;
            AuthenticatedUserService = authenticationUser;
            this.configuration = configuration;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
        .UseNpgsql(configuration.GetConnectionString("DocumentManagerConnection"))
        .UseSnakeCaseNamingConvention();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>(entity =>
            {
                entity.ToTable(name: "user");
            });

            builder.Entity<Group>(entity =>
            {
                entity.ToTable(name: "group");
            });

            builder.Entity<UserGroup>(entity =>
            {
                entity.ToTable(name: "user_group");
            });
            builder.Entity<UserGroup>()
            .HasKey(ug => new { ug.UserId, ug.GroupId });

            builder.Entity<UserGroup>()
            .HasOne<User>(u_g => u_g.User)
            .WithMany(u => u.UserGroups)
            .HasForeignKey(u_g => u_g.UserId);

            builder.Entity<UserGroup>()
            .HasOne<Group>(u_g => u_g.Group)
            .WithMany(g => g.UserGroups)
            .HasForeignKey(u_g => u_g.GroupId);

            builder.Entity<RefreshToken>(entity =>
            {
                entity.ToTable(name: "refresh_token");
            });
            builder.Entity<IdentityRole<long>>(entity =>
            {
                entity.ToTable(name: "role");
            });
            builder.Entity<IdentityUserRole<long>>(entity =>
            {
                entity.ToTable(name: "user_roles");
            });
            builder.Entity<IdentityUserClaim<long>>(entity =>
            {
                entity.ToTable(name: "user_claims");
            });
            builder.Entity<IdentityUserLogin<long>>(entity =>
            {
                entity.ToTable(name: "user_logins");
            });
            builder.Entity<IdentityRoleClaim<long>>(entity =>
            {
                entity.ToTable(name: "role_claims");
            });
            builder.Entity<IdentityUserToken<long>>(entity =>
            {
                entity.ToTable(name: "user_tokens");
            });
            builder.Entity<Documents>(entity =>
           {
               entity.ToTable(name: "documents");
           });

        }
    }
}