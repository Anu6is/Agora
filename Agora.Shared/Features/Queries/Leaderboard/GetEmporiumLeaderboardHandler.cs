using Agora.Shared.Persistence.Models;
using Agora.Shared.Persistence.Specifications;
using AutoMapper;
using Emporia.Application.Common;
using Emporia.Application.Specifications;
using Emporia.Persistence.DataAccess;

namespace Agora.Shared.Features.Queries
{
    internal class GetEmporiumLeaderboardHandler : IQueryHandler<GetEmporiumLeaderboardQuery, PagedResponse<LeaderboardResponse>>
    {
        private readonly IDataAccessor _dataAccessor;
        private readonly IMapper _mapper;

        public GetEmporiumLeaderboardHandler(IDataAccessor dataAccessor, IMapper mapper)
        {
            _mapper = mapper;
            _dataAccessor = dataAccessor;
        }

        public async Task<PagedResponse<LeaderboardResponse>> Handle(GetEmporiumLeaderboardQuery request, CancellationToken cancellationToken)
        {
            var economyUsers = await _dataAccessor.Transaction<GenericRepository<DefaultEconomyUser>>()
                                                  .ListAsync(new LeaderboardSpec(request.Filter), cancellationToken);

            var count = await _dataAccessor.Transaction<GenericRepository<DefaultEconomyUser>>()
                                           .CountAsync(new EntitySpec<DefaultEconomyUser>(x => x.EmporiumId == request.Filter.EmporiumId), cancellationToken);

            var data = _mapper.Map<List<LeaderboardResponse>>(economyUsers);

            return request.Filter.CreatePaginatedResponse(data, count);
        }
    }
}
