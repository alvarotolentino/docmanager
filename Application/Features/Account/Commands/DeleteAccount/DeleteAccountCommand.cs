using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.DeleteAccount
{
    public class DeleteAccountCommand : IRequest<Response<bool>>
    {
        public long id { get; set; }
    }

    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, Response<bool>>
    {

        IAccountRepositoryAsync accountRepository;
        public DeleteAccountCommandHandler(IAccountRepositoryAsync accountRepository)
        {
            this.accountRepository = accountRepository;
        }
        public async Task<Response<bool>> Handle(DeleteAccountCommand command, CancellationToken cancellationToken)
        {
            var result = await this.accountRepository.DeleteAccountById(command.id);
            return new Response<bool>(result);

        }
    }
}