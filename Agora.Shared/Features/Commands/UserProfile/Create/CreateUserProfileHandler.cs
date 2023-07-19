using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Services;

namespace Agora.Shared.Features.Commands
{
    internal class CreateUserProfileHandler : ICommandHandler<CreateUserProfileCommand, IResult<UserProfile>>
    {
        private readonly IDataAccessor _dataAccessor;

        public CreateUserProfileHandler(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public Task<IResult<UserProfile>> Handle(CreateUserProfileCommand command, CancellationToken cancellationToken)
        {
            var profile = UserProfile.Create(command.EmporiumId, command.UserId, command.UserReference);

            _dataAccessor.Create(profile);

            return Result.Success(profile);
        }
    }
}
