using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;
using Emporia.Domain.Services;

namespace Agora.Shared.Features.Commands
{
    public class UpdateUserProfileCommand : Command<IResult<UserProfile>>
    {
        public UserProfile Profile { get; }

        public UpdateUserProfileCommand(UserProfile profile)
        {
            Profile = profile;
        }
    }
}
