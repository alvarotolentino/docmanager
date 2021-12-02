using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IGroupRepositoryAsync
    {
        Task<int> CreateGroup(Group group, CancellationToken cancellationToken);
        Task<bool> DeleteGroup(Group group, CancellationToken cancellationToken);
        Task<Group> Update(Group group, CancellationToken cancellationToken);
        Task<Group> GetById(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Group>> GetGroups(int pageNumber, int pageSize, CancellationToken cancellationToken);
    }
}