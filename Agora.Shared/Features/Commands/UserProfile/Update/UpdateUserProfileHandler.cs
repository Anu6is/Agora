using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Persistence.DataAccess;

namespace Agora.Shared.Features.Commands
{
    internal class UpdateUserProfileHandler : ICommandHandler<UpdateUserProfileCommand, UserProfile>
    {
        private readonly IDataAccessor _dataAccessor;

        public UpdateUserProfileHandler(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public async Task<UserProfile> Handle(UpdateUserProfileCommand command, CancellationToken cancellationToken)
        {
            await _dataAccessor.Transaction<GenericRepository<UserProfile>>().UpdateAsync(command.Profile, cancellationToken);

            return command.Profile;
        }
    }
}
