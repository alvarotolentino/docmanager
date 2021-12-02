using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Exceptions;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Groups.Queries.GetGroupById
{
    public class GetGroupByIdQuery : IRequest<Response<GetGroupByIdViewModel>>
    {
        public int Id { get; set; }
    }

    public class GetGroupByIdQueryHandler : IRequestHandler<GetGroupByIdQuery, Response<GetGroupByIdViewModel>>
    {
        private const string ERRORTITLE = "Group Error";
        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public GetGroupByIdQueryHandler(IGroupRepositoryAsync groupRepositoryAsync, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepositoryAsync;
            this.mapper = mapper;
        }
        public async Task<Response<GetGroupByIdViewModel>> Handle(GetGroupByIdQuery query, CancellationToken cancellationToken)
        {
            var group = await this.groupRepositoryAsync.GetById(query.Id, cancellationToken);
            if (group == null) return new Response<GetGroupByIdViewModel>(null, message: "Group not found.", succeeded: false);
            var groupViewModel = this.mapper.Map<GetGroupByIdViewModel>(group);
            return new Response<GetGroupByIdViewModel>(groupViewModel);

        }
    }
}