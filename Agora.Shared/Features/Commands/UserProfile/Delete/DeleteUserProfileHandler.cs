using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Persistence.DataAccess;
using MediatR;

namespace Agora.Shared.Features.Commands
{
    internal class DeleteUserProfileHandler : ICommandHandler<DeleteUserProfileCommand, Unit>
    {
        private readonly IDataAccessor _dataAccessor;

        public DeleteUserProfileHandler(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public async Task<Unit> Handle(DeleteUserProfileCommand command, CancellationToken cancellationToken)
        {
            await _dataAccessor.Transaction<GenericRepository<UserProfile>>().DeleteAsync(UserProfile.Create(command.UserId), cancellationToken);

            return Unit.Value;
        }
    }
}
