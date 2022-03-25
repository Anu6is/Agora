using Microsoft.Extensions.Logging;

namespace Agora.Discord.Services
{
    public abstract class AgoraService
    {
        public readonly ILogger Logger;
        public AgoraService(ILogger logger) => Logger = logger;
    }
}
