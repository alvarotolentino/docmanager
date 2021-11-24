using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IGroupRepositoryAsync
    {
        Task<long> CreateGroup(Group group);
        Task<bool> DeleteGroup(long id);
        Task<Group> Update(Group group);
        Task<Group> GetById(long id);
        Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize);
    }
}