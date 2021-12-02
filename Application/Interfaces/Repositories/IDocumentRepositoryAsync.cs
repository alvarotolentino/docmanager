using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IDocumentRepositoryAsync
    {
        Task<Domain.Entities.Document> GetDocumentInfoById(int id, CancellationToken cancellationToken);
        Task<Domain.Entities.Document> GetDocumentDataById(int id, CancellationToken cancellationToken);
        Task<int> SaveDocument(Domain.Entities.Document document, CancellationToken cancellationToken);
        Task<bool> DeleteDocumentById(int id, CancellationToken cancellationToken);
        Task<IReadOnlyList<Domain.Entities.Document>> GetDocuments(int pageNumber, int pageSize, CancellationToken cancellationToken);
        Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken);
        Task<GroupDocument> AssingGroupPermissionAsync(GroupDocument groupDocument, CancellationToken cancellationToken);
    }
}