using Emporia.Domain.Common;
using Emporia.Extensions.Discord;

namespace Agora.Shared.Persistence.Models
{
    public class UserProfile : Entity<UserId>, IUserProfile
    {
        public EmporiumId EmporiumId { get; private set; }
        public ReferenceNumber UserReference { get; private set; }
        public bool OutbidAlerts { get; private set; }
        public bool TradeDealAlerts { get; private set; }
        public ulong Reviews { get; private set; }
        public decimal Rating { get; private set; }

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

        public UserProfile SetOutbidNotifications(bool enabled = true)
        {
            OutbidAlerts = enabled;

            return this;
        }

        public UserProfile SetTradeDealNotifications(bool enabled = true)
        {
            TradeDealAlerts = enabled;

            return this;
        }

        public UserProfile SetReviewCount(ulong count)
        {
            Reviews = count;

            return this;
        }

        public UserProfile SetRating(decimal rating)
        {
            Rating = rating;

            return this;
        }
    }
}
