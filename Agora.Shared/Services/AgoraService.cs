using Microsoft.Extensions.Logging;

namespace Agora.Shared.Services
{
    public abstract class AgoraService
    {
        public readonly ILogger Logger;
        public AgoraService(ILogger logger) => Logger = logger;
    }
}
