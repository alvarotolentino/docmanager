using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IAccountRepositoryAsync
    {
         Task<bool> DeleteAccountById(long id);

         Task<User> AddUserToGroup(UserGroup userGroup);

         Task<User> AssignRole(long userId, long  roleId);
    }
}