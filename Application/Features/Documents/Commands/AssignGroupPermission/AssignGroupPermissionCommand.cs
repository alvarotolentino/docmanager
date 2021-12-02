using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.AssignGroupPermission
{
    public class AssignGroupPermission : IRequest<Response<AssignGroupPermissionViewModel>>
    {
        public long DocumentId { get; set; }
        public long GroupId { get; set; }
    }

    public class AssignGroupPermissionHandler : IRequestHandler<AssignGroupPermission, Response<AssignGroupPermissionViewModel>>
    {
        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public AssignGroupPermissionHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<AssignGroupPermissionViewModel>> Handle(AssignGroupPermission request, CancellationToken cancellationToken)
        {
            var groupDocument = this.mapper.Map<GroupDocument>(request);
            var result = await this.documentRepositoryAsync.AssingGroupPermissionAsync(groupDocument, cancellationToken);
            if (result == null) return new Response<AssignGroupPermissionViewModel>(null, "Group or document not found.", succeeded: false);
            var assignGroupPermissionViewModel = this.mapper.Map<AssignGroupPermissionViewModel>(result);
            return new Response<AssignGroupPermissionViewModel>(assignGroupPermissionViewModel);
        }
    }
}