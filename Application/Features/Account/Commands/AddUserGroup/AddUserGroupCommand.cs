using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Account.Commands.AddUserGroup
{
    public class AddUserGroupCommand : IRequest<Response<AddUserGroupViewModel>>
    {
        public long userid { get; set; }
        public long groupid { get; set; }
    }

    public class AddUserGroupCommandHandler : IRequestHandler<AddUserGroupCommand, Response<AddUserGroupViewModel>>
    {

        private readonly IAccountRepositoryAsync accountRepository;
        private readonly IMapper mapper;
        public AddUserGroupCommandHandler(IAccountRepositoryAsync accountRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
        }
        public async Task<Response<AddUserGroupViewModel>> Handle(AddUserGroupCommand command, CancellationToken cancellationToken)
        {
            var userGroup = this.mapper.Map<UserGroup>(command);
            var result = await this.accountRepository.AddUserToGroup(userGroup);
            var addUserGroupViewModel = this.mapper.Map<AddUserGroupViewModel>(result);
            return new Response<AddUserGroupViewModel>(addUserGroupViewModel);
        }
    }
}