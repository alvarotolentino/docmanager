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
    public class UpdateGroupCommand : IRequest<Response<UpdateGroupViewModel>>
    {
        public long id { get; set; }
        public string name { get; set; }
    }

    public class UpdateGroupCommandHandler : IRequestHandler<UpdateGroupCommand, Response<UpdateGroupViewModel>>
    {
        private const string ERRORTITLE = "Group Error";

        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public UpdateGroupCommandHandler(IGroupRepositoryAsync groupRepository, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
        }
        public async Task<Response<UpdateGroupViewModel>> Handle(UpdateGroupCommand command, CancellationToken cancellationToken)
        {
            var request = this.mapper.Map<Group>(command);
            var group = await this.groupRepositoryAsync.Update(request, cancellationToken);
            if (group == null) throw new NotFoundException(ERRORTITLE);
            var groupViewModel = this.mapper.Map<UpdateGroupViewModel>(group);
            return new Response<UpdateGroupViewModel>(data: groupViewModel);
        }
    }
}