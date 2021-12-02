using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentById : IRequest<Response<DeleteDocumentViewModel>>
    {
        public int Id { get; set; }
    }

    public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentById, Response<DeleteDocumentViewModel>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public DeleteDocumentHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<DeleteDocumentViewModel>> Handle(DeleteDocumentById request, CancellationToken cancellationToken)
        {
            var document = this.mapper.Map<Document>(request);
            var result = await this.documentRepositoryAsync.DeleteDocumentById(document, cancellationToken);
            var deleteDocumentViewModel = result ? new DeleteDocumentViewModel() { Id = request.Id } : null;
            return new Response<DeleteDocumentViewModel>(deleteDocumentViewModel, message: result ? null : "Document not found");

        }
    }
}