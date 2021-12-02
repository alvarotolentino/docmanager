using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentInfoByIdQuery : IRequest<Response<GetDocumentInfoViewModel>>
    {
        public int Id { get; set; }
    }
    public class GetDocumentInfoByIdQueryHandler : IRequestHandler<GetDocumentInfoByIdQuery, Response<GetDocumentInfoViewModel>>
    {
        private const string ERRORTITLE = "Document Error";
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public GetDocumentInfoByIdQueryHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper, IAuthenticatedUserService authenticatedUserService)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
            this.authenticatedUserService = authenticatedUserService;

        }
        public async Task<Response<GetDocumentInfoViewModel>> Handle(GetDocumentInfoByIdQuery query, CancellationToken cancellationToken)
        {
            var userDocument = new UserDocument { DocumentId = query.Id, UserId = this.authenticatedUserService.UserId.Value };
            var document = await documentRepositoryAsync.GetDocumentInfoById(userDocument, cancellationToken);
            if (document == null) return new Response<GetDocumentInfoViewModel>(null, "Document not found or you do not have permissions.", succeeded: false);
            var documentViewModel = this.mapper.Map<GetDocumentInfoViewModel>(document);
            return new Response<GetDocumentInfoViewModel>(data: documentViewModel);
        }
    }
}