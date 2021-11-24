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

namespace Infrastructure.Persistence.Repositories
{
    public class AccountRepositoryAsync : IAccountRepositoryAsync
    {

        private NpgsqlConnection connection;
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public AccountRepositoryAsync(DbConnection dbConnection, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.connection = (NpgsqlConnection)dbConnection;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }
        public async Task<User> AddUserToGroup(UserGroup userGroup)
        {
            var param = new DynamicParameters();
            param.Add("p_userid", userGroup.UserId);
            param.Add("p_groupid", userGroup.GroupId);
            Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;
            var result = await this.connection.QueryAsync<dynamic>("udf_add_user_to_group", param, commandType: CommandType.StoredProcedure);
            var rows = result.Select(x => (IDictionary<string, object>)x).ToList();

            var user = new User { UserGroups = new List<UserGroup>() };
            foreach (var row in rows)
            {
                user.UserName = (string)row["user_name"];
                user.Id = (long)row["id"];
                user.UserGroups.Add(new UserGroup { Group = new Group { Id = (long)row["group_id"], Name = (string)row["name"] } });
            }
            return user;
        }

        public async Task<bool> DeleteAccountById(long id)
        {
            var param = new DynamicParameters();
            param.Add("@p_id", id);
            var affected = await this.connection.ExecuteAsync("CALL \"usp_delete_account\" (@p_id)", param);
            return true;
        }

        public async Task<User> AssignRole(long userId, long roleId)
        {
            var param = new DynamicParameters();
            param.Add("p_userid", userId);
            param.Add("p_roleid", roleId);
            var result = await this.connection.QueryAsync<dynamic>("udf_assig_role_to_user", param, commandType: CommandType.StoredProcedure);
            var rows = result.Select(x => (IDictionary<string, object>)x).ToList();
            var user = new User();
            foreach (var row in rows)
            {
                user.UserName = (string)row["user_name"];
                user.Id = (long)row["id"];
            }

            return user;
        }
    }
}