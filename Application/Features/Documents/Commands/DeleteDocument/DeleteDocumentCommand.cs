using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Enums;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using MediatR;

namespace Application.Features.Documents.Commands.DeleteDocument
{
    public class DeleteDocumentById : IRequest<Response<DeleteDocumentViewModel>>
    {
        public long Id { get; set; }
    }

    public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentById, Response<DeleteDocumentViewModel>>
    {
        private const string ERRORTITLE = "Document Error";
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        public DeleteDocumentHandler(IDocumentRepositoryAsync documentRepositoryAsync)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
        }
        public async Task<Response<DeleteDocumentViewModel>> Handle(DeleteDocumentById request, CancellationToken cancellationToken)
        {
            var result = await this.documentRepositoryAsync.DeleteDocumentById(request.Id, cancellationToken);
            var deleteDocumentViewModel = result ? new DeleteDocumentViewModel() { Id = request.Id } : null;
            return new Response<DeleteDocumentViewModel>(deleteDocumentViewModel, message: result ? null : "Document not found");

        }
    }
}