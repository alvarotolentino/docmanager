using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.DeleteGroup
{
    public class DeleteGroup : IRequest<Response<DeleteGroupViewModel>>
    {
        public int Id { get; set; }
    }

    public class DeleteGroupHandler : IRequestHandler<DeleteGroup, Response<DeleteGroupViewModel>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;

        public DeleteGroupHandler(IGroupRepositoryAsync groupRepository, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;

        }
        public async Task<Response<DeleteGroupViewModel>> Handle(DeleteGroup request, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(request);
            var result = await this.groupRepositoryAsync.DeleteGroup(group, cancellationToken);
            var deleteGroupViewModel = result ? new DeleteGroupViewModel() { Id = request.Id } : null;
            return new Response<DeleteGroupViewModel>(deleteGroupViewModel, message: result ? null : "Group not found");
        }
    }
}