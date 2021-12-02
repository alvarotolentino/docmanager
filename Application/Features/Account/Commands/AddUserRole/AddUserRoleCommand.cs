using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Application.Features.Account.Commands.AddUserRole
{
    public class AddUserRole : IRequest<Response<AddUserRoleViewModel>>
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }

    public class AddUserRoleHandler : IRequestHandler<AddUserRole, Response<AddUserRoleViewModel>>
    {
        private readonly IAccountRepositoryAsync accountRepository;
        private readonly IMapper mapper;
        public AddUserRoleHandler(IAccountRepositoryAsync accountRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
        }
        public async Task<Response<AddUserRoleViewModel>> Handle(AddUserRole request, CancellationToken cancellationToken)
        {
            var result = await this.accountRepository.AssignRole(new UserRole { UserId = request.UserId, RoleId = request.RoleId }, cancellationToken);
            if (result == null) return new Response<AddUserRoleViewModel>(null, message: "User or Role not found.", succeeded: false);
            var addUserRoleViewModel = this.mapper.Map<AddUserRoleViewModel>(result);
            return new Response<AddUserRoleViewModel>(addUserRoleViewModel, message: "Role assigned");
        }
    }
}