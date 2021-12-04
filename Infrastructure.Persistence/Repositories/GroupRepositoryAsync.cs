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
            using (var cmd = new NpgsqlCommand("CALL \"usp_insert_group\" (@p_id, @p_name, @p_created_by, @p_created_at)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_name", parameterType: NpgsqlDbType.Varchar, group.Name);
                cmd.Parameters.AddWithValue("@p_created_by", group.CreatedBy);
                cmd.Parameters.AddWithValue("@p_created_at", group.CreatedAt);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var value = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return value;
            }
        }

        public async Task<bool> DeleteGroup(Group group, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_group\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = group.Id, Direction = ParameterDirection.InputOutput });
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1;
            }
        }

        public async Task<Group> Update(Group group, CancellationToken cancellationToken)
        {
            var dbManager = new DbManager(connection);
            var sp = "CALL \"usp_update_group\" (@p_result, @p_id, @p_name, @p_updated_by, @p_updated_at)";
            var result = await dbManager.ExecuteNonQueryAsync<dynamic>(sp, cancellationToken, group, new { Result = -1 });
            

            using (var cmd = new NpgsqlCommand(, connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_result", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_id", group.Id);
                cmd.Parameters.AddWithValue("@p_name", group.Name);
                cmd.Parameters.AddWithValue("@p_updated_by", group.UpdatedBy);
                cmd.Parameters.AddWithValue("@p_updated_at", group.UpdatedAt);
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_result"].Value;
                connection.Close();
                return result > -1 ? new Group { Id = group.Id, Name = group.Name } : null;
            }
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
            return result.ToList();

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