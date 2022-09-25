using Agora.Shared.Persistence.Models;
using Emporia.Application.Common;

namespace Agora.Shared.Features.Commands
{
    public class UpdateUserProfileCommand : Command<UserProfile>
    {
        public UserProfile Profile { get; }

        public UpdateUserProfileCommand(UserProfile profile)
        {
            Profile = profile;
        }
    }
}
