using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Documents.Queries.DownloadDocumentById
{
    public class DownloadDocumentByIdQuery : IRequest<Response<DownloadDocumentViewModel>>
    {
        public int Id { get; set; }
    }

    public class DownloadDocumentByIdQueryHandler : IRequestHandler<DownloadDocumentByIdQuery, Response<DownloadDocumentViewModel>>
    {
        private const string ERRORTITLE = "Document Error";
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;

        public DownloadDocumentByIdQueryHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<DownloadDocumentViewModel>> Handle(DownloadDocumentByIdQuery query, CancellationToken cancellationToken)
        {
            var document = await documentRepositoryAsync.GetDocumentDataById(query.Id, cancellationToken);
            if (document == null) return new Response<DownloadDocumentViewModel>(null, "Document not found or you do not have permissions.", succeeded: false);
            var documentViewModel = this.mapper.Map<DownloadDocumentViewModel>(document);
            return new Response<DownloadDocumentViewModel>(data: documentViewModel);
        }
    }
}