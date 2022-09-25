using Emporia.Application.Common;
using Emporia.Domain.Common;

namespace Agora.Shared.Features.Commands
{
    public class DeleteUserProfileCommand : Command
    {
        public UserId UserId { get; set; }

        public DeleteUserProfileCommand(UserId userId)
        {
            UserId = userId;
        }
    }
}
