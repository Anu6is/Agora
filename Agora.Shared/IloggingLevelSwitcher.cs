using Serilog.Core;
using Serilog.Events;

namespace Agora.Shared
{
    public interface ILoggingLevelSwitcher
    {
        LoggingLevelSwitch DefaultLevelSwitch { get; }
        LoggingLevelSwitch EntityFrameworkLevelSwitch { get; }

        void SetMinimumLevelFromConfiguration();

        void SetMinimumLevel(LogEventLevel level);
        void SetOverrideLevel(LogEventLevel level);

    }
}
