using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Immutable;
using System.Reflection;
using ServiceLifetime = Agora.Shared.Attributes.AgoraServiceAttribute.ServiceLifetime;

namespace Agora.Shared.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureCustomAgoraServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) => services.ConfigureAgora().WithAgoraSharedServices());
        }

        public static IServiceCollection ConfigureAgora(this IServiceCollection services)
        {
            var types = Assembly.GetEntryAssembly().GetTypes().Where(type => type.IsAssignableTo(typeof(AgoraService)) && !type.IsAbstract).ToImmutableArray();

            foreach (Type serviceType in types)
                services.AddAgoraService(serviceType);

            return services;
        }

        public static IHostBuilder AddAgoraSharedServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, services) => services.WithAgoraSharedServices());
        }

        public static IServiceCollection WithAgoraSharedServices(this IServiceCollection services)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsAssignableTo(typeof(AgoraService)) && !type.IsAbstract).ToImmutableArray();

            foreach (Type serviceType in types)
                services.AddAgoraService(serviceType);

            services.AddFusionCache();

            return services;
        }

        public static void AddAgoraService(this IServiceCollection services, Type serviceType)
        {
            var implementationTypes = serviceType.GetImplementations().ToImmutableArray();
            var scope = serviceType.GetCustomAttribute<AgoraServiceAttribute>()?.Scope ?? ServiceLifetime.Singleton;

            if (implementationTypes.Length == 0)
                services.SetLifetime(scope, serviceType);
            else
                foreach (var implementationType in implementationTypes)
                    services.SetLifetime(scope, serviceType, implementationType);
        }

        private static IServiceCollection SetLifetime(this IServiceCollection services, ServiceLifetime scope, Type serviceType) => scope switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(serviceType),
            ServiceLifetime.Transient => services.AddTransient(serviceType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType),
            _ => services,
        };

        private static IServiceCollection SetLifetime(this IServiceCollection services, ServiceLifetime scope, Type serviceType, Type implementationType) => scope switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(implementationType, serviceType),
            ServiceLifetime.Transient => services.AddTransient(implementationType, serviceType),
            ServiceLifetime.Scoped => services.AddScoped(implementationType, serviceType),
            _ => services,
        };

        private static IEnumerable<Type> GetImplementations(this Type type)
        {
            if (type.BaseType == null)
                return type.GetInterfaces();
            else
                return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
        }
    }
}
