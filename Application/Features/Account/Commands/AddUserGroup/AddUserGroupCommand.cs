using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Account.Commands.AddUserGroup
{
    public class AddUserGroup : IRequest<Response<AddUserGroupViewModel>>
    {
        public int UserId { get; set; }
        public int GroupId { get; set; }
    }

    public class AddUserGroupHandler : IRequestHandler<AddUserGroup, Response<AddUserGroupViewModel>>
    {

        private readonly IAccountRepositoryAsync accountRepository;
        private readonly IMapper mapper;
        public AddUserGroupHandler(IAccountRepositoryAsync accountRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
        }
        public async Task<Response<AddUserGroupViewModel>> Handle(AddUserGroup request, CancellationToken cancellationToken)
        {
            var userGroup = this.mapper.Map<UserGroup>(request);
            var result = await this.accountRepository.AddUserToGroup(userGroup, cancellationToken);
            if (result == null) return new Response<AddUserGroupViewModel>(null, "User or group not found.", succeeded: false);
            var addUserGroupViewModel = this.mapper.Map<AddUserGroupViewModel>(result);
            return new Response<AddUserGroupViewModel>(addUserGroupViewModel);
        }
    }
}