using Agora.Shared.Extensions;
using Microsoft.Extensions.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace Agora.Shared.Services
{
    public class LoggingLevelSwitcher : ILoggingLevelSwitcher
    {
        public LoggingLevelSwitch LevelSwitch { get; }

        private readonly IConfiguration _configuration;
        public LoggingLevelSwitcher(IConfiguration configuration)
        {
            _configuration = configuration;
            LevelSwitch = new LoggingLevelSwitch();
        }

        public void SetMinimumLevelFromConfiguration()
        {
            var minLogLevel = _configuration.GetDefaultLogLevel();
            SetMinimumLevel(minLogLevel);
        }

        public void SetMinimumLevel(LogEventLevel level)
        {
            LevelSwitch.MinimumLevel = level;
        }
    }
}
