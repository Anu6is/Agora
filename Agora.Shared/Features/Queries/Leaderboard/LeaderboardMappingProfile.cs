using Agora.Shared.Persistence.Models;
using AutoMapper;

namespace Agora.Shared.Features.Queries
{
    public class LeaderboardMappingProfile : Profile
    {
        public LeaderboardMappingProfile()
        {
            CreateMap<DefaultEconomyUser, LeaderboardResponse>()
                .ForMember(r => r.EmporiumId, config => config.MapFrom(src => src.EmporiumId.Value))
                .ForMember(r => r.UserReference, config => config.MapFrom(src => src.UserReference.Value))
                .ForMember(r => r.Balance, config => config.MapFrom(src => src.Balance));
        }
    }
}
