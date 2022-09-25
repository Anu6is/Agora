using Emporia.Domain.Common;
using Emporia.Extensions.Discord;

namespace Agora.Shared.Persistence.Models
{
    public class UserProfile : Entity<UserId>, IUserProfile
    {
        public EmporiumId EmporiumId { get; private set; }
        public ReferenceNumber UserReference { get; private set; }
        public bool OutbidAlerts { get; set; }

        private UserProfile(UserId id) : base(id) { }

        public static UserProfile Create(UserId userId) => new UserProfile(userId);
        
        public static UserProfile Create(EmporiumId emporiumId, UserId userId, ReferenceNumber userReference) 
            => new UserProfile(userId) { EmporiumId = emporiumId, UserReference = userReference };

        public static UserProfile FromEmporiumUser(IEmporiumUser user)
        {
            var profile = new UserProfile(user.Id)
            {
                EmporiumId = user.EmporiumId,
                UserReference = user.ReferenceNumber,
            };

            return profile;
        }

        public UserProfile WithOutbidNotifications(bool enabled = true)
        {
            OutbidAlerts = enabled;

            return this;
        }
    }
}
