using Agora.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Agora.Shared.Services
{
    public class LoggingLevelSwitcher : ILoggingLevelSwitcher
    {
        public LoggingLevelSwitch DefaultLevelSwitch { get; }
        public LoggingLevelSwitch EntityFrameworkLevelSwitch { get; }


        private readonly IConfiguration _configuration;
        public LoggingLevelSwitcher(IConfiguration configuration)
        {
            _configuration = configuration;
            DefaultLevelSwitch = new LoggingLevelSwitch();
            EntityFrameworkLevelSwitch = new LoggingLevelSwitch();
        }

        public void SetMinimumLevelFromConfiguration()
        {
            var minLogLevel = _configuration.GetDefaultLogLevel();
            var entityLogLevel = _configuration.GetOverrideLoglevel("Microsoft.EntityFrameworkCore");

            SetMinimumLevel(minLogLevel);
            SetOverrideLevel(entityLogLevel);
        }

        public void SetMinimumLevel(LogEventLevel level)
        {
            DefaultLevelSwitch.MinimumLevel = level;
        }

        public void SetOverrideLevel(LogEventLevel level)
        {
            EntityFrameworkLevelSwitch.MinimumLevel = level;
        }
    }
}
