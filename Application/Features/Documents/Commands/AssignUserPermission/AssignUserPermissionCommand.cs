using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermissionCommand : IRequest<Response<AssignUserPermissionViewModel>>
    {
        public long DocumentId { get; set; }
        public long UserId { get; set; }
    }

    public class AssignUserPermissionsCommandHandler : IRequestHandler<AssignUserPermissionCommand, Response<AssignUserPermissionViewModel>>
    {

        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public AssignUserPermissionsCommandHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<Response<AssignUserPermissionViewModel>> Handle(AssignUserPermissionCommand command, CancellationToken cancellationToken)
        {
            var userDocument = this.mapper.Map<UserDocument>(command);
            var result = await documentRepositoryAsync.AssingUserPermissionAsync(userDocument, cancellationToken);
            if (result == null) return new Response<AssignUserPermissionViewModel>(null, "User or document not found.", succeeded: false);
            var assignUserPermissionViewModel = this.mapper.Map<AssignUserPermissionViewModel>(result);
            return new Response<AssignUserPermissionViewModel>(assignUserPermissionViewModel);
        }
    }

}