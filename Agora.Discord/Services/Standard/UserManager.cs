using Disqord.Bot;
using Emporia.Application.Common;
using Emporia.Domain.Common;
using Microsoft.Extensions.Logging;

namespace Agora.Discord.Services
{
    [AgoraService(AgoraServiceAttribute.ServiceLifetime.Scoped)]
    public class UserManager : AgoraService, IUserManager, ICurrentUserService
    {
        private readonly ICommandContextAccessor _accessor;
        public UserManager(ILogger<UserManager> logger, ICommandContextAccessor accesor) : base(logger) => _accessor = accesor;

        public ValueTask<bool> IsAdministrator(UserId userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> IsBroker(UserId userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> IsHost(UserId userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> ValidateBuyer(UserId userId)
        {
            throw new NotImplementedException();
        }

        public ValueTask<bool> ValidateUser(UserId userId)
        {
            throw new NotImplementedException();
        }
    }
}
