using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetAllDocumentsQuery : IRequest<PagedResponse<IEnumerable<GetAllDocumentsViewModel>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResponse<IEnumerable<GetAllDocumentsViewModel>>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public GetAllDocumentsQueryHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<GetAllDocumentsViewModel>>> Handle(GetAllDocumentsQuery query, CancellationToken cancellationToken)
        {
            var filter = this.mapper.Map<GetAllDocumentsParameter>(query);
            var documents = await this.documentRepositoryAsync.GetDocuments(filter.pagenumber, filter.pagesize, cancellationToken);
            if (documents == null) return new PagedResponse<IEnumerable<GetAllDocumentsViewModel>>(null, filter.pagenumber, filter.pagesize);
            var documentsViewModels = this.mapper.Map<IEnumerable<GetAllDocumentsViewModel>>(documents);
            return new PagedResponse<IEnumerable<GetAllDocumentsViewModel>>(documentsViewModels, filter.pagenumber, filter.pagesize);

        }
    }
}