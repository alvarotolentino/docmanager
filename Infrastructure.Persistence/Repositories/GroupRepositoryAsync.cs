using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Dapper;
using Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace Infrastructure.Persistence.Repositories
{
    public class GroupRepositoryAsync : IGroupRepositoryAsync
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
            var param = new DynamicParameters();
            param.Add("@p_id", direction: ParameterDirection.InputOutput);
            param.Add("@p_name", group.Name, DbType.String);
            param.Add("@p_created_by", this.authenticatedUserService.UserId, DbType.Int64);
            param.Add("@p_created_at", this.dateTimeService.UtcDateTime, DbType.DateTime);
            await this.connection.ExecuteAsync("CALL \"usp_insert_group\" (@p_id, @p_name, @p_created_by, @p_created_at)", param);

            var id = param.Get<long>("@p_id");
            return id;
        }

        public async Task<bool> DeleteGroup(long id)
        {
            var param = new DynamicParameters();
            param.Add("@p_id", id);
            var affected = await this.connection.ExecuteAsync("CALL \"usp_delete_group\" (@p_id)", param);
            return true;
        }

        public async Task<Group> Update(Group group)
        {
            var param = new DynamicParameters();
            param.Add("@p_id", group.Id);
            param.Add("@p_name", group.Name);
            param.Add("@p_updated_by", this.authenticatedUserService.UserId, DbType.Int64);
            param.Add("@p_updated_at", this.dateTimeService.UtcDateTime, DbType.DateTime);
            var affected = await this.connection.ExecuteAsync("CALL \"usp_update_group\" (@p_id, @p_name, @p_updated_by, @p_updated_at)", param);
            return new Group { Id = group.Id, Name = group.Name };
        }

        public async Task<Group> GetById(long id)
        {
            var param = new DynamicParameters();
            param.Add("p_id", id);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var group = await this.connection.QuerySingleOrDefaultAsync<Group>("udf_get_group_by_id", param, commandType: CommandType.StoredProcedure);
            return group;
        }

        public async Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize)
        {
            var param = new DynamicParameters();
            param.Add("p_number", pageNumber);
            param.Add("p_size", pageSize);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var group = await this.connection.QueryAsync<Group>("udf_get_groups_by_page_number_size", param, commandType: CommandType.StoredProcedure);
            return group.ToList();
        }
    }
}