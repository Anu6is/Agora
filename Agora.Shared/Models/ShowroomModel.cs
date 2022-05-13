using Emporia.Domain.Common;

namespace Agora.Shared.Models
{
    public class ShowroomModel
    {
        public ulong ShowroomId { get; set; }
        public TimeSpan OpensAt { get; set; }
        public TimeSpan ClosesAt { get; set; }
        public ListingType ListingType { get; set; }
        public bool IsActive { get; set; }

        public ShowroomModel(ulong showroomId)
        {
            ShowroomId = showroomId;
        }
    }
}
