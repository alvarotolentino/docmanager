using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Groups.Queries.GetGroups
{
    public class GetGroupsQuery : IRequest<PagedResponse<IEnumerable<GetGroupsViewModel>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetGroupsQueryHandler : IRequestHandler<GetGroupsQuery, PagedResponse<IEnumerable<GetGroupsViewModel>>>
    {

        private readonly IGroupRepositoryAsync groupRepositoryAsync;
        private readonly IMapper mapper;
        public GetGroupsQueryHandler(IGroupRepositoryAsync groupRepositoryAsync, IMapper mapper)
        {
            this.groupRepositoryAsync = groupRepositoryAsync;
            this.mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<GetGroupsViewModel>>> Handle(GetGroupsQuery query, CancellationToken cancellationToken)
        {
            var filter = this.mapper.Map<GetAllGroupsParameter>(query);
            var groups = await this.groupRepositoryAsync.GetGroups(filter.pagenumber, filter.pagesize);
            var groupsViewModels = groups != null ? this.mapper.Map<IEnumerable<GetGroupsViewModel>>(groups) : Enumerable.Empty<GetGroupsViewModel>();
            return new PagedResponse<IEnumerable<GetGroupsViewModel>>(groupsViewModels, filter.pagenumber, filter.pagesize);
        }
    }
}