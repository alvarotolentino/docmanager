using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.RegisterGroup
{
    public class RegisterGroupCommand : IRequest<Response<RegisterGroupViewModel>>
    {
        public string name { get; set; }
    }

    public class RegisterGroupCommandHandler : IRequestHandler<RegisterGroupCommand, Response<RegisterGroupViewModel>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public RegisterGroupCommandHandler(IGroupRepositoryAsync groupRepository, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
        }
        public async Task<Response<RegisterGroupViewModel>> Handle(RegisterGroupCommand command, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(command);
            var id = await this.groupRepositoryAsync.CreateGroup(group);
            return new Response<RegisterGroupViewModel>(new RegisterGroupViewModel { Id = id, Name = group.Name });
        }
    }
}