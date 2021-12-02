using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.DeleteGroup
{
    public class DeleteGroup : IRequest<Response<DeleteGroupViewModel>>
    {
        public long Id { get; set; }
    }

    public class DeleteGroupHandler : IRequestHandler<DeleteGroup, Response<DeleteGroupViewModel>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;

        public DeleteGroupHandler(IGroupRepositoryAsync groupRepository)
        {
            this.groupRepositoryAsync = groupRepository;

        }
        public async Task<Response<DeleteGroupViewModel>> Handle(DeleteGroup request, CancellationToken cancellationToken)
        {
            var result = await this.groupRepositoryAsync.DeleteGroup(request.Id, cancellationToken);
            var deleteGroupViewModel = result ? new DeleteGroupViewModel() { Id = request.Id } : null;
            return new Response<DeleteGroupViewModel>(deleteGroupViewModel, message: result ? null : "Group not found");
        }
    }
}