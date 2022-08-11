using Agora.Shared.EconomyFactory.Models;
using Ardalis.Specification;
using Emporia.Domain.Common;

namespace Agora.Shared.Persistence.Specifications
{
    public class EconomyUsersSpec : Specification<DefaultEconomyUser>
    {
        public EconomyUsersSpec(EmporiumId emporiumId)
        {
            Query.Where(x => x.EmporiumId == emporiumId);
        }
    }
}
