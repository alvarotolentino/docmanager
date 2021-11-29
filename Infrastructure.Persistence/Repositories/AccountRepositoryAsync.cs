using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Infrastructure.Persistence.Repositories
{
    public class AccountRepositoryAsync : IAccountRepositoryAsync
    {
        private NpgsqlConnection connection;
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public AccountRepositoryAsync(DbConnection dbConnection,
        IAuthenticatedUserService authenticatedUserService,
        IDateTimeService dateTimeService)
        {
            this.connection = (NpgsqlConnection)dbConnection;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }
        public async Task<User> AddUserToGroup(UserGroup userGroup, CancellationToken cancellationToken)
        {
            var user = new User() { Groups = new List<Group>() };
            using (var cmd = new NpgsqlCommand("udf_add_user_to_group", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_userid", userGroup.UserId);
                cmd.Parameters.AddWithValue("p_groupid", userGroup.GroupId);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        user.Id = (long)reader["id"];
                        user.UserName = reader["user_name"].ToString();
                        var group = new Group();
                        group.Id = (long)reader["group_id"];
                        group.Name = reader["name"].ToString();
                        user.Groups.Add(group);
                    }
                }
                connection.Close();
            }
            return user;
        }

        public async Task<bool> DeleteAccountById(long id, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_account\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.AddWithValue("@p_id", id);
                var affected = await cmd.ExecuteNonQueryAsync(cancellationToken);
                connection.Close();
                return true;
            }
        }

        public async Task<User> AssignRole(long userId, long roleId, CancellationToken cancellationToken)
        {
            var user = new User() { Roles = new List<Role>() };
            using (var cmd = new NpgsqlCommand("udf_assig_role_to_user", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_userid", userId);
                cmd.Parameters.AddWithValue("p_roleid", roleId);
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.HasRows)
                    {
                        reader.Read();
                        user.Id = (long)reader["id"];
                        user.UserName = reader["user_name"].ToString();
                        var role = new Role();
                        role.Id = (long)reader["role_id"];
                        role.Name = reader["name"].ToString();
                        user.Roles.Add(role);
                    }
                }
            }
            return user;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_create_account_default_role", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("p_id", DbType.Int64) { Direction = ParameterDirection.Output });
                cmd.Parameters.Add(new NpgsqlParameter("p_user_exists", DbType.Boolean) { Direction = ParameterDirection.Output });
                cmd.Parameters.AddWithValue("p_first_name", user.FirstName);
                cmd.Parameters.AddWithValue("p_last_name", user.LastName);
                cmd.Parameters.AddWithValue("p_user_name", user.UserName);
                cmd.Parameters.AddWithValue("p_normalized_user_name", user.UserName.ToUpper());
                cmd.Parameters.AddWithValue("p_email", user.Email);
                cmd.Parameters.AddWithValue("p_normalized_email", user.Email.ToUpper());
                cmd.Parameters.AddWithValue("p_password_hashed", user.PasswordHash);
                cmd.Parameters.AddWithValue("p_created_at", this.dateTimeService.UtcDateTime);
                cmd.Parameters.AddWithValue("p_created_by", this.authenticatedUserService.UserId);

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                connection.Close();
                var alreadyExists = (bool)cmd.Parameters["p_user_exists"].Value;
                var id = (long)cmd.Parameters["p_id"].Value;

                return alreadyExists ? IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Email '{user.Email}' is already registered." } }) : IdentityResult.Success;
            }
        }

        public async Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {

            using (var cmd = new NpgsqlCommand(@"
            SELECT * FROM udf_find_user_by_email (@p_email);
            SELECT * FROM udf_find_user_roles_by_email (@p_email);
            SELECT * FROM udf_find_user_groups_by_email (@p_email)
            ", connection))
            {
                var user = new User() { Roles = new List<Role>(), Groups = new List<Group>() };
                connection.Open();
                cmd.Parameters.AddWithValue("p_email", normalizedEmail);
                cmd.Prepare();
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {

                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    user.Id = (long)reader["user_id"];
                    user.FirstName = reader["first_name"].ToString();
                    user.LastName = reader["last_name"].ToString();
                    user.UserName = reader["user_name"].ToString();
                    user.Email = reader["email"].ToString();
                    user.PasswordHash = reader["password_hash"].ToString();

                    if (reader.NextResult() && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var role = new Role();
                            role.Id = (long)reader["id"];
                            role.Name = reader["name"].ToString();
                            user.Roles.Add(role);
                        }
                    }

                    if (reader.NextResult() && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var group = new Group();
                            group.Id = (long)reader["id"];
                            group.Name = reader["name"].ToString();
                            user.Groups.Add(group);
                        }
                    }
                }
                connection.Close();
                return user;
            }
        }

        public void Dispose()
        {
            if (this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}