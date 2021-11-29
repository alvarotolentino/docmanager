using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IDocumentRepositoryAsync
    {
        Task<Domain.Entities.Documents> GetDocumentInfoById(long id, CancellationToken cancellationToken);
        Task<Domain.Entities.Documents> GetDocumentDataById(long id, CancellationToken cancellationToken);
        Task<long> SaveDocument(Domain.Entities.Documents document, CancellationToken cancellationToken);
        Task<bool> DeleteDocumentById(long id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Domain.Entities.Documents>> GetDocuments(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken);
        Task<GroupDocument> AssingGroupPermissionAsync(GroupDocument groupDocument, CancellationToken cancellationToken);
    }
}