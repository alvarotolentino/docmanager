using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermissionCommand : IRequest<Response<AssignGroupPermissionViewModel>>
    {
        public long DocumentId { get; set; }
        public long GroupId { get; set; }
    }

    public class AssignGroupPermissionCommandHandler : IRequestHandler<AssignGroupPermissionCommand, Response<AssignGroupPermissionViewModel>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public AssignGroupPermissionCommandHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<AssignGroupPermissionViewModel>> Handle(AssignGroupPermissionCommand command, CancellationToken cancellationToken)
        {
            var groupDocument = this.mapper.Map<GroupDocument>(command);
            var result = await this.documentRepositoryAsync.AssingGroupPermissionAsync(groupDocument, cancellationToken);
            if (result == null) return new Response<AssignGroupPermissionViewModel>(null, "Group or document not found.", succeeded: false);
            var assignGroupPermissionViewModel = this.mapper.Map<AssignGroupPermissionViewModel>(result);
            return new Response<AssignGroupPermissionViewModel>(assignGroupPermissionViewModel);
        }
    }
}