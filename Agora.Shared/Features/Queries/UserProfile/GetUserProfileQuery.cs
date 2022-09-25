using Emporia.Application.Common;
using Emporia.Domain.Common;

namespace Agora.Shared.Features.Queries
{
    public class GetUserProfileQuery : Query<Response<UserProfileResponse>>
    {
        public EmporiumId EmporiumId { get; set; }
        public ReferenceNumber ReferenceNumber { get; set; }

        public GetUserProfileQuery(EmporiumId emporiumId, ReferenceNumber referenceNumber)
        {
            EmporiumId = emporiumId;
            ReferenceNumber = referenceNumber;
        }
    }
}
