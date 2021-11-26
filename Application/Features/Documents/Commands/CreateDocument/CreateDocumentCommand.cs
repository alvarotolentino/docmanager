using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Application.Features.Documents.Commands.CreateDocument
{
    public class CreateDocumentCommand : IRequest<Response<long>>
    {
        public string description { get; set; }
        public string category { get; set; }
        public IFormFile file { get; set; }

    }

    public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand, Response<long>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;

        public CreateDocumentCommandHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<Response<long>> Handle(CreateDocumentCommand command, CancellationToken cancellationToken)
        {
            var document = this.mapper.Map<Domain.Entities.Documents>(command);
            var id = await this.documentRepositoryAsync.SaveDocument(document, cancellationToken);
            return new Response<long>(id);
        }
    }
}