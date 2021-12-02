using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Features.Documents.Queries.GetAllDocuments;
using Domain.Entities;

namespace Application.Interfaces.Repositories
{
    public interface IDocumentRepositoryAsync
    {
        Task<Domain.Entities.Document> GetDocumentInfoById(UserDocument userDocument, CancellationToken cancellationToken);
        Task<Domain.Entities.Document> GetDocumentDataById(UserDocument userDocument, CancellationToken cancellationToken);
        Task<int> SaveDocument(Domain.Entities.Document document, CancellationToken cancellationToken);
        Task<bool> DeleteDocumentById(Document document, CancellationToken cancellationToken);
        Task<IReadOnlyList<Domain.Entities.Document>> GetDocuments(GetUserDocumentsPaginated userDocumentsPaginated, CancellationToken cancellationToken);
        Task<UserDocument> AssingUserPermissionAsync(UserDocument userDocument, CancellationToken cancellationToken);
        Task<GroupDocument> AssingGroupPermissionAsync(GroupDocument groupDocument, CancellationToken cancellationToken);
    }
}