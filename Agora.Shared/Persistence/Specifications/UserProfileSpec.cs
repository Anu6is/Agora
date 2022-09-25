using Agora.Shared.Persistence.Models;
using Ardalis.Specification;
using Emporia.Domain.Common;

namespace Agora.Shared.Persistence.Specifications
{
    public class UserProfileSpec : Specification<UserProfile>
    {
        public UserProfileSpec(EmporiumId emporiumId, ReferenceNumber userReference)
        {
            Query.Where(x => x.EmporiumId == emporiumId && x.UserReference == userReference);
        }
    }
}
