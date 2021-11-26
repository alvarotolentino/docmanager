using System.Threading.Tasks;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Application.Interfaces.Repositories
{
    public interface IAccountRepositoryAsync :
    IUserStore<User>,
    IUserRoleStore<User>,
    IUserPasswordStore<User>,
    IUserEmailStore<User>
    {
        Task<bool> DeleteAccountById(long id);

        Task<User> AddUserToGroup(UserGroup userGroup);

        Task<User> AssignRole(long userId, long roleId);
    }
}