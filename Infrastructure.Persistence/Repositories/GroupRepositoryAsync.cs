using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public GroupRepositoryAsync(DbConnection dbConnection, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.connection = (NpgsqlConnection)dbConnection;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }

        public async Task<long> CreateGroup(Group group)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_insert_group\" (@p_id, @p_name, @p_created_by, @p_created_at)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int64) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_name", parameterType: NpgsqlDbType.Varchar, group.Name);
                cmd.Parameters.AddWithValue("@p_created_by", this.authenticatedUserService.UserId);
                cmd.Parameters.AddWithValue("@p_created_at", this.dateTimeService.UtcDateTime);
                await cmd.ExecuteNonQueryAsync();
                var id = (long)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return id;
            }
        }

        public async Task<bool> DeleteGroup(long id)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_group\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@p_id", id);
                var result = await cmd.ExecuteNonQueryAsync();
                connection.Close();
                return true;
            }
        }

        public async Task<Group> Update(Group group)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_update_group\" (@p_result, @p_id, @p_name, @p_updated_by, @p_updated_at)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_result", DbType.Int32) { Value = -1, Direction = ParameterDirection.InputOutput });
                cmd.Parameters.AddWithValue("@p_id", group.Id);
                cmd.Parameters.AddWithValue("@p_name", group.Name);
                cmd.Parameters.AddWithValue("@p_updated_by", this.authenticatedUserService.UserId);
                cmd.Parameters.AddWithValue("@p_updated_at", this.dateTimeService.UtcDateTime);
                await cmd.ExecuteNonQueryAsync();
                var result = (int)cmd.Parameters["@p_result"].Value;
                connection.Close();
                return result == 1 ? new Group { Id = group.Id, Name = group.Name } : null;
            }
        }

        public async Task<Group> GetById(long id)
        {
            using (var cmd = new NpgsqlCommand("udf_get_group_by_id", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_id", id);
                cmd.Prepare();
                Group group = null;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        group = new Group();
                        group.Id = (long)reader["id"];
                        group.Name = reader["name"].ToString();
                        group.CreatedBy = (long)reader["created_by"];
                        group.CreatedAt = (DateTime)reader["created_at"];
                        group.UpdatedBy = (long)reader["updated_by"];
                        group.UpdatedAt = (DateTime)reader["updated_at"];

                    }
                }
                connection.Close();
                return group;
            }
        }

        public async Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize)
        {
            using (var cmd = new NpgsqlCommand("udf_get_groups_by_page_number_size", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_number", pageNumber);
                cmd.Parameters.AddWithValue("p_size", pageSize);
                cmd.Prepare();
                List<Group> groups = null;
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (reader.HasRows)
                    {
                        groups = new List<Group>();
                        while (reader.Read())
                        {
                            var group = new Group
                            {
                                Id = (long)reader["id"],
                                Name = reader["name"].ToString(),
                                CreatedBy = (long)reader["created_by"],
                                CreatedAt = (DateTime)reader["created_at"],
                                UpdatedBy = (long)reader["updated_by"],
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