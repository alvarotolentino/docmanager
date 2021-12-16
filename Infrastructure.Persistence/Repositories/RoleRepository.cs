using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Infrastructure.Persistence.Repositories
{
    public class RoleRepositoryAsync : IRoleRepositoryAsync
    {

        private NpgsqlConnection connection;
        public RoleRepositoryAsync(DatabaseConnections docManagerConnection)
        {
            this.connection = docManagerConnection.MetadataConnection;
        }
        public async Task<IdentityResult> CreateAsync(Role role, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var dyn = await dbManager.ExecuteNonQueryAsync<dynamic>("udf_create_role", cancellationToken,
            inputParam: role,
            outpuParam: new { Id = -1 },
            commandType: CommandType.StoredProcedure);
            if (dyn == null) return IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Role cannot be created." } });

            var keyValue = dyn as IDictionary<string, object>;
            var id = (int)keyValue["Id"];
            return id > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Role '{role.Name}' already exists." } });

        }

        public async Task<IdentityResult> DeleteAsync(Role role, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var dyn = await dbManager.ExecuteNonQueryAsync<dynamic>("CALL \"usp_delete_role\" (@p_id)", cancellationToken,
            outpuParam: new { Id = role.Id });
            if (dyn == null) return IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Role cannot be deleted." } });

            var keyValue = dyn as IDictionary<string, object>;
            var id = (int)keyValue["Id"];
            return id > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Role not found." } });
        }

        public void Dispose()
        {
        }

        public Task<Role> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return Task.FromResult<Role>(null);
        }

        public Task<Role> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return Task.FromResult<Role>(null);

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
            return Task.FromResult<IdentityResult>(null);

        }
    }
}