using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using AutoMapper;
using Domain.Entities;
using MediatR;

namespace Application.Features.Groups.Commands.RegisterGroup
{
    public class RegisterGroup : IRequest<Response<RegisterGroupViewModel>>
    {
        public string name { get; set; }
    }

    public class RegisterGroupHandler : IRequestHandler<RegisterGroup, Response<RegisterGroupViewModel>>
    {
        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        private readonly IAuthenticatedUserService authenticatedUserService;
        private readonly IDateTimeService dateTimeService;
        public RegisterGroupHandler(IGroupRepositoryAsync groupRepository, IMapper mapper, IAuthenticatedUserService authenticatedUserService, IDateTimeService dateTimeService)
        {
            this.groupRepositoryAsync = groupRepository;
            this.mapper = mapper;
            this.authenticatedUserService = authenticatedUserService;
            this.dateTimeService = dateTimeService;
        }
        public async Task<Response<RegisterGroupViewModel>> Handle(RegisterGroup request, CancellationToken cancellationToken)
        {
            var group = this.mapper.Map<Group>(request);
            group.CreatedBy = this.authenticatedUserService.UserId.Value;
            group.CreatedAt = this.dateTimeService.UtcDateTime;
            var id = await this.groupRepositoryAsync.CreateGroup(group, cancellationToken);
            return new Response<RegisterGroupViewModel>(new RegisterGroupViewModel { Id = id, Name = group.Name });
        }
    }
}