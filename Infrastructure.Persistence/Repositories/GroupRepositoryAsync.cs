using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Database;
using Npgsql;
using NpgsqlTypes;

namespace Infrastructure.Persistence.Repositories
{
    public class GroupRepositoryAsync : IGroupRepositoryAsync, IDisposable
    {

        private NpgsqlConnection connection;

        public GroupRepositoryAsync(DatabaseConnections docManagerConnection)
        {
            this.connection = docManagerConnection.MetadataConnection;
        }

        public async Task<int> CreateGroup(Group group, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var sp = "CALL \"usp_insert_group\" (@p_id, @p_name, @p_created_by, @p_created_at)";
            var dyn = await dbManager.ExecuteNonQueryAsync<dynamic>(sp, cancellationToken, group, new { Id = -1 });
            if (dyn == null) return await Task.FromResult(0);
            var keyValue = dyn as IDictionary<string, object>;
            return (int)keyValue["Id"];
        }

        public async Task<bool> DeleteGroup(Group group, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var sp = "CALL \"usp_delete_group\" (@p_id)";
            var dyn = await dbManager.ExecuteNonQueryAsync<dynamic>(sp, cancellationToken, group, new { Id = -1 });
            if (dyn == null) return false;
            var keyValue = dyn as IDictionary<string, object>;
            var id = (int)keyValue["Id"];
            return id > -1;
        }

        public async Task<Group> Update(Group group, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var sp = "CALL \"usp_update_group\" (@p_result, @p_id, @p_name, @p_updated_by, @p_updated_at)";
            var dyn = await dbManager.ExecuteNonQueryAsync<dynamic>(sp, cancellationToken, group, outpuParam: new { Result = -1 });
            if (dyn == null) return null;

            var groupUpdated = new Group() { Id = group.Id, Name = group.Name };
            var keyValue = dyn as IDictionary<string, object>;
            var result = (int)keyValue["Result"];
            return result == -1 ? null : groupUpdated;

        }

        public async Task<Group> GetById(int id, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var result = await dbManager.ExecuteReaderAsync<Group>("udf_get_group_by_id", cancellationToken, inputParam: new { Id = id }, commandType: CommandType.StoredProcedure);
            return result;
        }

        public async Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {

            using var dbManager = new DbManager(connection);
            var result = await dbManager.ExecuteReaderAsListAsync<Group>("udf_get_groups_by_page_number_size",
            cancellationToken,
            inputParam: new { Number = pageNumber, Size = pageSize },
            commandType: CommandType.StoredProcedure
            );
            return result != null ? result.ToList() : null;

        }

        public void Dispose()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }
    }
}