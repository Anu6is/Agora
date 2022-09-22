using Emporia.Application.Specifications;
using Emporia.Domain.Common;

namespace Agora.Shared.Persistence.Specifications.Filters
{
    public class LeaderboardFilter : BaseFilter
    {
        public EmporiumId EmporiumId { get; private set; }

        public LeaderboardFilter(EmporiumId emporiumId)
        {
            EmporiumId = emporiumId;
        }
    }
}
