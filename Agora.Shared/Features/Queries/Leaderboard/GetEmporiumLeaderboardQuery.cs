using Agora.Shared.Persistence.Specifications.Filters;
using Emporia.Application.Common;

namespace Agora.Shared.Features.Queries
{
    public class GetEmporiumLeaderboardQuery : Query<PagedResponse<LeaderboardResponse>>
    {
        public LeaderboardFilter Filter { get; }

        public GetEmporiumLeaderboardQuery(LeaderboardFilter filter)
        {
            Filter = filter;
        }
    }
}
