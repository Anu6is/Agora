using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Services;
using Emporia.Persistence.DataAccess;
using MediatR;

namespace Agora.Shared.Features.Commands
{
    internal class DeleteUserProfileHandler : ICommandHandler<DeleteUserProfileCommand, IResult<Unit>>
    {
        private readonly IDataAccessor _dataAccessor;

        public DeleteUserProfileHandler(IDataAccessor dataAccessor)
        {
            _dataAccessor = dataAccessor;
        }

        public async Task<IResult<Unit>> Handle(DeleteUserProfileCommand command, CancellationToken cancellationToken)
        {
            await _dataAccessor.Transaction<GenericRepository<UserProfile>>().DeleteAsync(UserProfile.Create(command.UserId), cancellationToken);

            return Result.Success(Unit.Value);
        }
    }
}
