using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Documents.Commands.AssignUserPermission
{
    public class AssignUserPermission : IRequest<Response<AssignUserPermissionViewModel>>
    {
        public int DocumentId { get; set; }
        public int UserId { get; set; }
    }

    public class AssignUserPermissionsHandler : IRequestHandler<AssignUserPermission, Response<AssignUserPermissionViewModel>>
    {

        private readonly IDocumentRepositoryAsync documentRepositoryAsync;
        private readonly IMapper mapper;
        public AssignUserPermissionsHandler(IDocumentRepositoryAsync documentRepositoryAsync, IMapper mapper)
        {
            this.documentRepositoryAsync = documentRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<Response<AssignUserPermissionViewModel>> Handle(AssignUserPermission request, CancellationToken cancellationToken)
        {
            var userDocument = this.mapper.Map<UserDocument>(request);
            var result = await documentRepositoryAsync.AssingUserPermissionAsync(userDocument, cancellationToken);
            if (result == null) return new Response<AssignUserPermissionViewModel>(null, "User or document not found.", succeeded: false);
            var assignUserPermissionViewModel = this.mapper.Map<AssignUserPermissionViewModel>(result);
            return new Response<AssignUserPermissionViewModel>(assignUserPermissionViewModel);
        }
    }

}