﻿using Agora.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sentry.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace Agora.Shared.Extensions
{
    public static class LoggerExtensions
    {
        public static ILoggingBuilder ReplaceDefaultLogger(this ILoggingBuilder builder)
        {
            builder.Services.Remove(builder.Services.First(descriptor => descriptor.ImplementationType == typeof(ConsoleLoggerProvider)));
            return builder;
        }

        public static ILoggingBuilder WithSerilog(this ILoggingBuilder builder, HostBuilderContext context)
        {
            var switcher = new LoggingLevelSwitcher(context.Configuration);
            
            builder.Services.AddSingleton<ILoggingLevelSwitcher>(switcher);
            switcher.SetMinimumLevelFromConfiguration();

            builder.AddSerilog(
                new LoggerConfiguration()
                    .ReadFrom.Configuration(context.Configuration)
                    .MinimumLevel.ControlledBy(switcher.LevelSwitch).CreateLogger(), 
                dispose: false);
            return builder;
        }

        public static ILoggingBuilder AddSentry(this ILoggingBuilder builder, HostBuilderContext context)
        {
            builder.Services.Configure<SentryLoggingOptions>(context.Configuration.GetSection("Sentry"));
            builder.AddSentry();
            return builder;
        }

        public static LogEventLevel GetDefaultLogLevel(this IConfiguration configuration)
        {
            return configuration.GetValue<LogEventLevel>("Serilog:MinimumLevel:Default");
        }
    }
}
