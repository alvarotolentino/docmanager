using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Interfaces.Repositories
{
    public interface IDocumentRepositoryAsync
    {
        Task<Domain.Entities.Documents> GetDocumentInfoById(long id);
        Task<Domain.Entities.Documents> GetDocumentDataById(long id);
        Task<long> SaveDocument(Domain.Entities.Documents document);
        Task<bool> DeleteDocumentById(long id);

        Task<IReadOnlyList<Domain.Entities.Documents>> GetDocuments(int pageNumber, int pageSize);

    }
}