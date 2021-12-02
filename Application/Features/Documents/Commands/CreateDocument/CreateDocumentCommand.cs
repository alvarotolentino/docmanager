using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
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
        private readonly IDateTimeService dateTimeService;
        private readonly IAuthenticatedUserService authenticatedUserService;

        public CreateDocumentHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
            this.dateTimeService = dateTimeService;
            this.authenticatedUserService = authenticatedUserService;
        }

        public async Task<Response<CreateDocumentViewModel>> Handle(CreateDocument request, CancellationToken cancellationToken)
        {
            var document = this.mapper.Map<Domain.Entities.Document>(request);
            document.CreatedBy = this.authenticatedUserService.UserId.Value;
            document.CreatedAt = this.dateTimeService.UtcDateTime;
            var id = await this.documentRepositoryAsync.SaveDocument(document, cancellationToken);
            if (id == -1) return new Response<CreateDocumentViewModel>(null, message: "Document cannot be saved", succeeded: false);
            return new Response<CreateDocumentViewModel>(new CreateDocumentViewModel { Id = id, Name = document.Name });
        }
    }
}