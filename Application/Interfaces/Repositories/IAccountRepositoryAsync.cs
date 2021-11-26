using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IAccountRepositoryAsync :
    IUserStore<User>
    {
        Task<bool> DeleteAccountById(long id, CancellationToken cancellationToken);

        Task<User> AddUserToGroup(UserGroup userGroup, CancellationToken cancellationToken);

        Task<User> AssignRole(long userId, long roleId, CancellationToken cancellationToken);

        Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken);
    }
}