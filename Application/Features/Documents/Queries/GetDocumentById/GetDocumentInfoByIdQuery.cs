using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Documents.Queries.GetDocumentById
{
    public class GetDocumentInfoByIdQuery : IRequest<Response<GetDocumentInfoViewModel>>
    {
        public long id { get; set; }
    }
    public class GetDocumentInfoByIdQueryHandler : IRequestHandler<GetDocumentInfoByIdQuery, Response<GetDocumentInfoViewModel>>
    {
        private const string ERRORTITLE = "Document Error";
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public GetDocumentInfoByIdQueryHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<GetDocumentInfoViewModel>> Handle(GetDocumentInfoByIdQuery query, CancellationToken cancellationToken)
        {
            var document = await documentRepositoryAsync.GetDocumentInfoById(query.id);
            if (document == null) throw new NotFoundException(ERRORTITLE);
            var documentViewModel = this.mapper.Map<GetDocumentInfoViewModel>(document);
            return new Response<GetDocumentInfoViewModel>(data: documentViewModel);
        }
    }
}