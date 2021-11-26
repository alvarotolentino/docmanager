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
    public class DeleteDocumentByIdCommand : IRequest<Response<long>>
    {
        public long Id { get; set; }
    }

    public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentByIdCommand, Response<long>>
    {
        private const string ERRORTITLE = "Document Error";
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        public DeleteDocumentCommandHandler(IDocumentRepositoryAsync documentRepositoryAsync)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
        }
        public async Task<Response<long>> Handle(DeleteDocumentByIdCommand command, CancellationToken cancellationToken)
        {
            var result = await this.documentRepositoryAsync.DeleteDocumentById(command.Id, cancellationToken);
            return new Response<long>(result ? command.Id : 0);


        }
    }
}