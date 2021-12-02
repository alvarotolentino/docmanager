using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.UpdateGroup
{
    public class UpdateGroup : IRequest<Response<UpdateGroupViewModel>>
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class UpdateGroupHandler : IRequestHandler<UpdateGroup, Response<UpdateGroupViewModel>>
    {
        private const string ERRORTITLE = "Group Error";

        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public UpdateGroupHandler(IGroupRepositoryAsync groupRepository, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
        }
        public async Task<Response<UpdateGroupViewModel>> Handle(UpdateGroup request, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(request);
            var groupUpdated = await this.groupRepositoryAsync.Update(group, cancellationToken);
            if (groupUpdated == null) return new Response<UpdateGroupViewModel>(null, message: "Group not found", succeeded: false);
            var groupViewModel = this.mapper.Map<UpdateGroupViewModel>(groupUpdated);
            return new Response<UpdateGroupViewModel>(data: groupViewModel);
        }
    }
}