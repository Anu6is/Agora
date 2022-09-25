using Agora.Shared.Persistence.Models;
using Agora.Shared.Persistence.Specifications;
using AutoMapper;
using Emporia.Application.Common;
using Emporia.Persistence.DataAccess;

namespace Agora.Shared.Features.Queries
{
    internal class GetUserProfileHandler : IQueryHandler<GetUserProfileQuery, Response<UserProfileResponse>>
    {
        private readonly IDataAccessor _dataAccessor;
        private readonly IMapper _mapper;

        public GetUserProfileHandler(IDataAccessor dataAccessor, IMapper mapper)
        {
            _mapper = mapper;
            _dataAccessor = dataAccessor;
        }

        public async Task<Response<UserProfileResponse>> Handle(GetUserProfileQuery request, CancellationToken cancellationToken)
        {
            var profile = await _dataAccessor.Transaction<GenericRepository<UserProfile>>()
                                             .FirstOrDefaultAsync(new UserProfileSpec(request.EmporiumId, request.ReferenceNumber), cancellationToken);
            var data = _mapper.Map<UserProfileResponse>(profile);
            var response = new Response<UserProfileResponse>(data);

            return response;
        }
    }
}
