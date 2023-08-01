using Agora.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentry;
using Sentry.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Agora.Shared.Extensions
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder ReplaceDefaultLogger(this ILoggingBuilder builder)
        {
            builder.ClearProviders();

            return builder;
        }

        public static ILoggingBuilder WithSerilog(this ILoggingBuilder builder, HostBuilderContext context)
        {
            var levelSwitcher = new LoggingLevelSwitcher(context.Configuration);

            builder.Services.AddSingleton<ILoggingLevelSwitcher>(levelSwitcher);

            levelSwitcher.SetMinimumLevelFromConfiguration();

            builder.AddSerilog(
                new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.ControlledBy(levelSwitcher.DefaultLevelSwitch)
                    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", levelSwitcher.EntityFrameworkLevelSwitch)
                    .CreateLogger(),
                dispose: false);
            return builder;
        }

        public static ILoggingBuilder AddSentry(this ILoggingBuilder builder,
                                                HostBuilderContext context,
                                                Func<SentryEvent, SentryEvent> sentryCallback = null)
        {
            builder.Services.Configure<SentryLoggingOptions>(context.Configuration.GetSection("Sentry"));
            builder.AddSentry(options => options.SetBeforeSend(sentryCallback));
            return builder;
        }

        public static LogEventLevel GetDefaultLogLevel(this IConfiguration configuration)
        {
            return configuration.GetValue<LogEventLevel>("Serilog:MinimumLevel:Default");
        }

        public static LogEventLevel GetOverrideLoglevel(this IConfiguration configuration, string logger)
        {
            return configuration.GetValue<LogEventLevel>($"Serilog:MinimumLevel:Override:{logger}");
        }
    }
}
