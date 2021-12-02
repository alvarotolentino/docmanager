using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.BaseParameters;
using Application.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using MediatR;

namespace Application.Features.Documents.Queries.GetAllDocuments
{
    public class GetAllDocumentsQuery : PagedParameter, IRequest<PagedResponse<IEnumerable<GetAllDocumentsViewModel>>>
    {

    }

    public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, PagedResponse<IEnumerable<GetAllDocumentsViewModel>>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IAuthenticatedUserService authenticatedUserService;
        private readonly IMapper mapper;
        public GetAllDocumentsQueryHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper, IAuthenticatedUserService authenticatedUserService)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.authenticatedUserService = authenticatedUserService;
            this.mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<GetAllDocumentsViewModel>>> Handle(GetAllDocumentsQuery query, CancellationToken cancellationToken)
        {
            var userDocumentsPaginated = new GetUserDocumentsPaginated
            {
                PageNumber = query.PageNumber,
                PageSize = query.PageSize,
                UserId = this.authenticatedUserService.UserId.Value
            };
            var documents = await this.documentRepositoryAsync.GetDocuments(userDocumentsPaginated, cancellationToken);
            if (documents == null) return new PagedResponse<IEnumerable<GetAllDocumentsViewModel>>(null, query.PageNumber, query.PageSize);
            var documentsViewModels = this.mapper.Map<IEnumerable<GetAllDocumentsViewModel>>(documents);
            return new PagedResponse<IEnumerable<GetAllDocumentsViewModel>>(documentsViewModels, query.PageNumber, query.PageSize);

        }
    }
}