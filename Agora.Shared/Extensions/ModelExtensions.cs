using Agora.Shared.Models;
using Emporia.Application.Features.Queries;

namespace Agora.Shared.Extensions
{
    public static class ModelExtensions
    {
        public static ShowroomModel ToShowroomModel(this ShowroomDetailsResponse response) => new ShowroomModel(response.ShowroomId.Value)
        {
            ItemType = response.ItemType,
            OpensAt = response.OpensAt,
            ClosesAt = response.ClosesAt,
            IsActive = response.IsActive
        };
        
        public static string BusinessHours(this ShowroomModel response)
        {
            if (response.OpensAt == response.ClosesAt) return "24-hours";

            return $"{response.OpensAt:HH:mm} - {response.ClosesAt:HH:mm}";
        }
    }
}
