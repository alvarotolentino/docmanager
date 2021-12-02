using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.RegisterGroup
{
    public class RegisterGroup : IRequest<Response<RegisterGroupViewModel>>
    {
        public string name { get; set; }
    }

    public class RegisterGroupHandler : IRequestHandler<RegisterGroup, Response<RegisterGroupViewModel>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public RegisterGroupHandler(IGroupRepositoryAsync groupRepository, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
        }
        public async Task<Response<RegisterGroupViewModel>> Handle(RegisterGroup request, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(request);
            var id = await this.groupRepositoryAsync.CreateGroup(group, cancellationToken);
            return new Response<RegisterGroupViewModel>(new RegisterGroupViewModel { Id = id, Name = group.Name });
        }
    }
}