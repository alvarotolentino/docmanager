using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Account.Queries.GetAccounts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IAccountRepositoryAsync :
    IUserStore<User>
    {

        Task<User> AddUserToGroup(UserGroup userGroup, CancellationToken cancellationToken);

        Task<User> AssignRole(UserRole userRole, CancellationToken cancellationToken);

        Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
        Task<IReadOnlyList<User>> GetAccounts(GetAllAccountsParameter filter, CancellationToken cancellationToken);
    }
}