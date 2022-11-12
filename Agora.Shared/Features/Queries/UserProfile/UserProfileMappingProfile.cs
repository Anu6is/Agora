using Agora.Shared.Persistence.Models;
using AutoMapper;

namespace Agora.Shared.Features.Queries
{
    public class UserProfileMappingProfile : Profile
    {
        public UserProfileMappingProfile()
        {
            CreateMap<UserProfile, UserProfileResponse>()
                .ForMember(r => r.UserId, config => config.MapFrom(src => src.Id.Value))
                .ForMember(r => r.EmporiumId, config => config.MapFrom(src => src.EmporiumId.Value))
                .ForMember(r => r.ReferenceNumber, config => config.MapFrom(src => src.UserReference.Value))
                .ForMember(r => r.TradeDealAlerts, config => config.MapFrom(src => src.TradeDealAlerts))
                .ForMember(r => r.OutbidAlerts, config => config.MapFrom(src => src.OutbidAlerts))
                .ForMember(r => r.Reviews, config => config.MapFrom(src => src.Reviews))
                .ForMember(r => r.Rating, config => config.MapFrom(src => src.Rating));
        }
    }
}
