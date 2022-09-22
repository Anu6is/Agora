using Agora.Shared.EconomyFactory.Models;
using Agora.Shared.Persistence.Specifications.Filters;
using Ardalis.Specification;
using Emporia.Application.Specifications;

namespace Agora.Shared.Persistence.Specifications
{
    public class LeaderboardSpec : Specification<DefaultEconomyUser>
    {
        public LeaderboardSpec(LeaderboardFilter filter)
        {
            Query.Where(x => x.EmporiumId == filter.EmporiumId && x.Balance > 0)
                 .OrderByDescending(x => x.Balance)
                 .Paginate(filter)
                 .AsNoTracking();

        }
    }
}
