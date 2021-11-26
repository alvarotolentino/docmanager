using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.DeleteGroup
{
    public class DeleteGroupCommand : IRequest<Response<bool>>
    {
        public long id { get; set; }
    }

    public class DeleteGroupCommandHandler : IRequestHandler<DeleteGroupCommand, Response<bool>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;

        public DeleteGroupCommandHandler(IGroupRepositoryAsync groupRepository)
        {
            this.groupRepositoryAsync = groupRepository;

        }
        public async Task<Response<bool>> Handle(DeleteGroupCommand command, CancellationToken cancellationToken)
        {
            var result = await this.groupRepositoryAsync.DeleteGroup(command.id, cancellationToken);
            return new Response<bool>(data: result, message: result ? "Group and all users associated with were deleted successfully." : "Group was not found or deleted.");
        }
    }
}