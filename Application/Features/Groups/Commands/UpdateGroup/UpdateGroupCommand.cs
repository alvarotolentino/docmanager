using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.UpdateGroup
{
    public class UpdateGroup : IRequest<Response<UpdateGroupViewModel>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class UpdateGroupHandler : IRequestHandler<UpdateGroup, Response<UpdateGroupViewModel>>
    {
        private const string ERRORTITLE = "Group Error";

        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        private readonly IAuthenticatedUserService authenticatedUserService;
        private readonly IDateTimeService dateTimeService;
        public UpdateGroupHandler(IGroupRepositoryAsync groupRepository, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
            this.authenticatedUserService = authenticatedUserService;
            this.dateTimeService = dateTimeService;
        }
        public async Task<Response<UpdateGroupViewModel>> Handle(UpdateGroup request, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(request);
            group.UpdatedBy = this.authenticatedUserService.UserId.Value;
            group.UpdatedAt = this.dateTimeService.UtcDateTime;

            var groupUpdated = await this.groupRepositoryAsync.Update(group, cancellationToken);
            if (groupUpdated == null) return new Response<UpdateGroupViewModel>(null, message: "Group not found", succeeded: false);
            var groupViewModel = this.mapper.Map<UpdateGroupViewModel>(groupUpdated);
            return new Response<UpdateGroupViewModel>(data: groupViewModel);
        }
    }
}