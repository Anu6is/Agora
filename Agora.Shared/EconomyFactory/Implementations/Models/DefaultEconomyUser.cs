using Emporia.Domain.Common;

namespace Agora.Shared.EconomyFactory.Models
{
    public class DefaultEconomyUser : Entity
    {
        public UserId UserId { get; set; }
        public EmporiumId EmporiumId { get; set; }
        public ReferenceNumber UserReference { get; set; }
        public decimal Balance { get; set; }

        public static DefaultEconomyUser FromEmporiumUser(IEmporiumUser user)
        {
            var economyUser = new DefaultEconomyUser()
            {
                UserId = user.Id,
                EmporiumId = user.EmporiumId,
                UserReference = user.ReferenceNumber,
            };

            return economyUser;
        }

        public DefaultEconomyUser WithBalance(decimal amount)
        {
            Balance = amount;

            return this;
        }
    }
}
