using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Sentry.Extensions.Logging;
using Serilog;

namespace Agora.Discord.Logging
{
    internal static class LoggerExtensions
    {
        public static ILoggingBuilder ReplaceDefaultLogger(this ILoggingBuilder builder)
        {
            builder.Services.Remove(builder.Services.First(descriptor => descriptor.ImplementationType == typeof(ConsoleLoggerProvider)));
            return builder;
        }

        public static ILoggingBuilder WithSerilog(this ILoggingBuilder builder, HostBuilderContext context)
        {
            builder.AddSerilog(new LoggerConfiguration().ReadFrom.Configuration(context.Configuration).CreateLogger(), dispose: false);
            return builder;
        }

        public static ILoggingBuilder AddSentry(this ILoggingBuilder builder, HostBuilderContext context)
{
            builder.Services.Configure<SentryLoggingOptions>(context.Configuration.GetSection("Sentry"));
            builder.AddSentry();

            return builder;
        }
    }
}
