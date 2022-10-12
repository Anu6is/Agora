namespace Agora.Shared.Features.Queries
{
    public class UserProfileResponse
    {
        public Guid UserId { get; set; }
        public ulong EmporiumId { get; set; }
        public ulong ReferenceNumber { get; set; }
        public bool OutbidAlerts { get; set; }
        public ulong Reviews { get; set; }
        public decimal Rating { get; set; }
    }
}
