using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.DeleteAccount
{
    public class DeleteAccount : IRequest<Response<bool>>
    {
        public int Id { get; set; }
    }

    public class DeleteAccountHandler : IRequestHandler<DeleteAccount, Response<bool>>
    {

        IAccountRepositoryAsync accountRepository;
        public DeleteAccountHandler(IAccountRepositoryAsync accountRepository)
        {
            this.accountRepository = accountRepository;
        }
        public async Task<Response<bool>> Handle(DeleteAccount request, CancellationToken cancellationToken)
        {
            var result = await this.accountRepository.DeleteAsync(new User { Id = request.Id }, cancellationToken);
            if (result != IdentityResult.Success) return new Response<bool>(false, message: result.Errors.FirstOrDefault().Description, succeeded: false);
            return new Response<bool>(true, message: $"User with Id {request.Id} deleted successfully.");

        }
    }
}