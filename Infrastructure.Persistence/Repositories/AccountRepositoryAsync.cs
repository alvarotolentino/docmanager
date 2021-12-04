using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Exceptions;
using Application.Features.Account.Queries.GetAccounts;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;
using Infrastructure.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Npgsql;

namespace Infrastructure.Persistence.Repositories
{
    public class AccountRepositoryAsync : IAccountRepositoryAsync
    {
        private NpgsqlConnection connection;

        public AccountRepositoryAsync(DatabaseConnections docManagerConnection,
        IAuthenticatedUserService authenticatedUserService,
        IDateTimeService dateTimeService)
        {
            this.connection = docManagerConnection.MetadataConnection;
        }
        public async Task<User> AddUserToGroup(UserGroup userGroup, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var dyn = await dbManager.ExecuteReaderAsync<dynamic>("udf_add_user_to_group", cancellationToken, inputParam: userGroup, commandType: CommandType.StoredProcedure);
            if (dyn == null) return null;

            var user = new User() { Groups = new List<Group>() };
            var keyValue = dyn as IDictionary<string, object>;
            user.Id = (int)keyValue[nameof(user.Id)];
            user.UserName = keyValue[nameof(user.UserName)].ToString();
            var group = new Group();
            group.Id = (int)keyValue["GroupId"];
            group.Name = keyValue[nameof(group.Name)].ToString();
            user.Groups.Add(group);

            return user;
        }

        public async Task<User> AssignRole(UserRole userRole, CancellationToken cancellationToken)
        {
            using var dbManager = new DbManager(connection);
            var dyn = await dbManager.ExecuteReaderAsync<dynamic>("udf_assig_role_to_user", cancellationToken, inputParam: userRole, commandType: CommandType.StoredProcedure);
            if (dyn == null) return null;

            var user = new User() { Roles = new List<Role>() };
            var keyValue = dyn as IDictionary<string, object>;
            user.Id = (int)keyValue[nameof(user.Id)];
            user.UserName = keyValue[nameof(user.UserName)].ToString();
            var role = new Role();
            role.Id = (int)keyValue["RoleId"];
            role.Name = keyValue[nameof(role.Name)].ToString();
            user.Roles.Add(role);
            return user;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("udf_create_account_default_role", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add(new NpgsqlParameter("p_id", DbType.Int32) { Value = -1, Direction = ParameterDirection.Output });
                cmd.Parameters.AddWithValue("p_first_name", user.FirstName);
                cmd.Parameters.AddWithValue("p_last_name", user.LastName);
                cmd.Parameters.AddWithValue("p_user_name", user.UserName);
                cmd.Parameters.AddWithValue("p_normalized_user_name", user.NormalizedUserName);
                cmd.Parameters.AddWithValue("p_email", user.Email);
                cmd.Parameters.AddWithValue("p_normalized_email", user.NormalizedEmail);
                cmd.Parameters.AddWithValue("p_password_hash", user.PasswordHash);
                cmd.Parameters.AddWithValue("p_created_at", user.CreatedAt);
                cmd.Parameters.AddWithValue("p_created_by", user.CreatedBy);
                cmd.Prepare();

                await cmd.ExecuteNonQueryAsync(cancellationToken);
                connection.Close();
                user.Id = (int)cmd.Parameters["p_id"].Value;

                return user.Id > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"Email '{user.Email}' already exists." } });
            }
        }

        public async Task<User> FindByEmailAsync(User user, CancellationToken cancellationToken)
        {

            using (var cmd = new NpgsqlCommand(@"
            SELECT * FROM udf_find_user_by_email (@p_email);
            SELECT * FROM udf_find_user_roles_by_email (@p_email);
            SELECT * FROM udf_find_user_groups_by_email (@p_email)
            ", connection))
            {
                var userFound = new User() { Roles = new List<Role>(), Groups = new List<Group>() };
                connection.Open();
                cmd.Parameters.AddWithValue("p_email", user.Email);
                cmd.Prepare();
                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {

                    if (!reader.HasRows)
                        return null;

                    reader.Read();
                    userFound.Id = (int)reader["user_id"];
                    userFound.FirstName = reader["first_name"].ToString();
                    userFound.LastName = reader["last_name"].ToString();
                    userFound.UserName = reader["user_name"].ToString();
                    userFound.Email = reader["email"].ToString();
                    userFound.PasswordHash = reader["password_hash"].ToString();

                    if (reader.NextResult() && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var role = new Role();
                            role.Id = (int)reader["id"];
                            role.Name = reader["name"].ToString();
                            userFound.Roles.Add(role);
                        }
                    }

                    if (reader.NextResult() && reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var group = new Group();
                            group.Id = (int)reader["id"];
                            group.Name = reader["name"].ToString();
                            userFound.Groups.Add(group);
                        }
                    }
                }
                connection.Close();
                return userFound;
            }
        }

        public void Dispose()
        {
            if (this.connection != null && this.connection.State == ConnectionState.Open)
            {
                this.connection.Close();
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            using (var cmd = new NpgsqlCommand("CALL \"usp_delete_account\" (@p_id)", connection))
            {
                connection.Open();
                cmd.Parameters.Add(new NpgsqlParameter("@p_id", DbType.Int32) { Value = user.Id, Direction = ParameterDirection.InputOutput });
                cmd.Prepare();
                await cmd.ExecuteNonQueryAsync(cancellationToken);
                var result = (int)cmd.Parameters["@p_id"].Value;
                connection.Close();
                return result > -1 ? IdentityResult.Success : IdentityResult.Failed(new IdentityError[] { new IdentityError { Description = $"User not found." } });
            }
        }

        public async Task<IReadOnlyList<User>> GetAccounts(GetAllAccountsParameter filter, CancellationToken cancellationToken)
        {
            List<User> users = null;
            using (var cmd = new NpgsqlCommand("udf_get_accounts_by_page_number_size", connection))
            {
                connection.Open();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_number", filter.PageNumber);
                cmd.Parameters.AddWithValue("p_size", filter.PageSize);
                cmd.Prepare();

                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    users = new List<User>();
                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            var user = new User
                            {
                                Id = (int)reader["id"],
                                Email = reader["email"].ToString()
                            };
                            users.Add(user);
                        }
                    }
                }

                connection.Close();
            }
            return users;
        }
        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return null;
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName ?? user.UserName.ToUpper());
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = user.UserName.ToUpper();
            return Task.FromResult(0);
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = user.NormalizedUserName.ToLower();
            return Task.FromResult(0);
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return null;
        }

    }
}