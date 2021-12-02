using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Application.Common;
using Application.Interfaces.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Account.Queries.GetAccounts
{
    public class GetAllAccountsQuery : IRequest<PagedResponse<IEnumerable<GetAllAccountsViewModel>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllAccountQueryHandler : IRequestHandler<GetAllAccountsQuery, PagedResponse<IEnumerable<GetAllAccountsViewModel>>>
    {

        private readonly IAccountRepositoryAsync accountRepository;
        private readonly IMapper mapper;

        public GetAllAccountQueryHandler(IAccountRepositoryAsync accountRepository, IMapper mapper)
        {
            this.accountRepository = accountRepository;
            this.mapper = mapper;
        }
        public async Task<PagedResponse<IEnumerable<GetAllAccountsViewModel>>> Handle(GetAllAccountsQuery query, CancellationToken cancellationToken)
        {
            var filter = this.mapper.Map<GetAllAccountsParameter>(query);
            var users = await this.accountRepository.GetAccounts(filter, cancellationToken);
            if (users == null) return new PagedResponse<IEnumerable<GetAllAccountsViewModel>>(null, filter.PageNumber, filter.PageSize);
            var accountViewModels = this.mapper.Map<IEnumerable<GetAllAccountsViewModel>>(users);
            return new PagedResponse<IEnumerable<GetAllAccountsViewModel>>(accountViewModels, filter.PageNumber, filter.PageSize);
        }
    }
}