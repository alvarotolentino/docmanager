using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepositoryAsync : IRoleRepositoryAsync
    {

        private NpgsqlConnection connection;
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;
        public RoleRepositoryAsync(DbConnection dbConnection,
        IAuthenticatedUserService authenticatedUserService,
        IDateTimeService dateTimeService)
        {
            this.connection = (NpgsqlConnection)dbConnection;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_create_role", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("p_id", DbType.Int32) { Direction = ParameterDirection.Output });
                cmd.Parameters.AddWithValue("p_name", role.Name);
                cmd.Parameters.AddWithValue("p_created_at", this.dateTimeService.UtcDateTime);
                cmd.Parameters.AddWithValue("p_created_by", this.authenticatedUserService.UserId);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var id = (int)cmd.Parameters["p_id"].Value;
                connection.Close();
                return id > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Role '{role.Name}' already exists." } });

            }
        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_role\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = role.Id, Direction = ParameterDirection.InputOutput });
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"User not found." } });
            }
        }

        public void Dispose()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<string> GetNormalizedRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.NormalizedName ?? role.Name.ToUpper());
        }

        public Task<string> GetRoleIdAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Id.ToString());
        }

        public Task<string> GetRoleNameAsync(Role role, CancellationToken cancellationToken)
        {
            return Task.FromResult(role.Name);
        }

        public Task SetNormalizedRoleNameAsync(Role role, string normalizedName, CancellationToken cancellationToken)
        {
            role.NormalizedName = role.Name.ToUpper();
            return Task.FromResult(0);
        }

        public Task SetRoleNameAsync(Role role, string roleName, CancellationToken cancellationToken)
        {
            role.Name = role.NormalizedName.ToLower();
            return Task.FromResult(0);
        }

        public Task<IdentityResult> UpdateAsync(Role role, CancellationToken cancellationToken)
        {
            return null;
        }
    }
}