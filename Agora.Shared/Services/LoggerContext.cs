using Emporia.Application.Common;

namespace Agora.Shared.Services
{
    public class LoggerContext : ILoggerContext
    {
        public Dictionary<string, object> ContextInfo { get; init; } = new();
    }
}
