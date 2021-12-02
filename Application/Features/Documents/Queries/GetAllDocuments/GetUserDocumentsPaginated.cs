using Application.BaseParameters;

namespace Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetUserDocumentsPaginated
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int UserId { get; set; }
    }
}