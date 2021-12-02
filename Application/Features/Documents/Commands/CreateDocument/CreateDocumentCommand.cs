using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocument : IRequest<Response<CreateDocumentViewModel>>
    {
        public string description { get; set; }
        public string category { get; set; }
        public IFormFile file { get; set; }

    }

    public class CreateDocumentHandler : IRequestHandler<CreateDocument, Response<CreateDocumentViewModel>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;

        public CreateDocumentHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<Response<CreateDocumentViewModel>> Handle(CreateDocument request, CancellationToken cancellationToken)
        {
            var document = this.mapper.Map<Domain.Entities.Documents>(request);
            var id = await this.documentRepositoryAsync.SaveDocument(document, cancellationToken);
            return new Response<CreateDocumentViewModel>(new CreateDocumentViewModel { Id = id, Name = document.Name });
        }
    }
}