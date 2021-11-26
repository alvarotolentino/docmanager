using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.AddUserRole
{
    public class AddUserRoleCommand : IRequest<Response<bool>>
    {
        public long userid { get; set; }
        public long roleid { get; set; }
    }

    public class AddUserRoleCommandHandler : IRequestHandler<AddUserRoleCommand, Response<bool>>
    {
        IAccountRepositoryAsync accountRepository;
        public AddUserRoleCommandHandler(IAccountRepositoryAsync accountRepository)
        {
            this.accountRepository = accountRepository;
        }
        public async Task<Response<bool>> Handle(AddUserRoleCommand command, CancellationToken cancellationToken)
        {
            var result = await this.accountRepository.AssignRole(command.userid, command.roleid, cancellationToken);
            return new Response<bool>(result != null, message: "Role assigned");
        }
    }
}