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
using Npgsql;
using NpgsqlTypes;

namespace Infrastructure.Persistence.Repositories
{
    public class GroupRepositoryAsync : IGroupRepositoryAsync, IDisposable
    {

        private NpgsqlConnection connection;

        public GroupRepositoryAsync(DbConnection dbConnection)
        {
            this.connection = (NpgsqlConnection)dbConnection;
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
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1;
            }
        }

        public async Task<Group> Update(Group group, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_update_group\" (@p_result, @p_id, @p_name, @p_updated_by, @p_updated_at)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_result", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_id", group.Id);
                cmd.Parameters.AddWithValue("@p_name", group.Name);
                cmd.Parameters.AddWithValue("@p_updated_by", group.UpdatedBy);
                cmd.Parameters.AddWithValue("@p_updated_at", group.UpdatedAt);
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_result"].Value;
                connection.Close();
                return result > -1 ? new Group { Id = group.Id, Name = group.Name } : null;
            }
        }

        public async Task<Group> GetById(int id, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_group_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.Prepare();
                Group group = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        group = new Group();
                        group.Id = (int)reader["id"];
                        group.Name = reader["name"].ToString();
                        group.CreatedBy = (int)reader["created_by"];
                        group.CreatedAt = (DateTime)reader["created_at"];
                        group.UpdatedBy = (int)reader["updated_by"];
                        group.UpdatedAt = (DateTime)reader["updated_at"];

                    }
                }
                connection.Close();
                return group;
            }
        }

        public async Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_get_groups_by_page_number_size", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_number", pageNumber);
                cmd.Parameters.AddWithValue("p_size", pageSize);
                cmd.Prepare();
                List<Group> groups = null;
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        groups = new List<Group>();
                        while (reader.Read())
                        {
                            var group = new Group
                            {
                                Id = (int)reader["id"],
                                Name = reader["name"].ToString(),
                                CreatedBy = (int)reader["created_by"],
                                CreatedAt = (DateTime)reader["created_at"],
                                UpdatedBy = (int)reader["updated_by"],
                                UpdatedAt = (DateTime)reader["updated_at"],
                            };
                            groups.Add(group);
                        }
                    }
                }
                connection.Close();
                return groups;
            }
        }

        public void Dispose()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }
    }
}