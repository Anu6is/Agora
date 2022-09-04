using Serilog.Core;
using Serilog.Events;

namespace Agora.Shared
{
    public interface ILoggingLevelSwitcher
    {
        LoggingLevelSwitch LevelSwitch { get; }

        void SetMinimumLevelFromConfiguration();

        void SetMinimumLevel(LogEventLevel level);
    }
}
