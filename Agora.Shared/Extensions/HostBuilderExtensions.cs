using Agora.Shared.Attributes;
using Agora.Shared.Services;
using Believe.Net;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Immutable;
using System.Reflection;
using ServiceLifetime = Agora.Shared.Attributes.AgoraServiceAttribute.ServiceLifetime;

namespace Agora.Shared.Extensions
{
    public static class HostBuilderExtensions
    {
        public static IList<Assembly> LoadCommandAssemblies(this IConfiguration configuration)
        {
            var assemblies = new List<Assembly>();
            var externalAssemblies = configuration.GetSection("Addons").GetChildren().Select(x => x.Value + ".dll").ToArray();

            foreach (var assemblyName in externalAssemblies)
            {
                var assembly = Assembly.LoadFrom(assemblyName);
                    
                if (assembly.GetTypes().Any(t => t.IsAssignableTo(typeof(ICommandModuleBase)) && !t.IsAbstract)) assemblies.Add(assembly);
            }

            return assemblies;
        }

        public static IHostBuilder ConfigureCustomAgoraServices(this IHostBuilder builder)
        {
            return builder.ConfigureServices((context, services)
                => services.ConfigureAgora().WithAgoraSharedServices(context.Configuration).AddEconomyServices(context.Configuration));
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
            return builder.ConfigureServices((context, services) => services.WithAgoraSharedServices(context.Configuration));
        }

        public static IServiceCollection WithAgoraSharedServices(this IServiceCollection services, IConfiguration configuration)
        {
            var types = Assembly.GetExecutingAssembly().GetTypes().Where(type => type.IsAssignableTo(typeof(AgoraService)) && !type.IsAbstract).ToImmutableArray();

            foreach (Type type in types)
                services.AddAgoraService(type);

            var externalAssemblies = configuration.GetSection("Addons").GetChildren().Select(x => x.Value + ".dll").ToArray();

            foreach (var extAssembly in externalAssemblies)
            {
                types = Assembly.LoadFrom(extAssembly).GetTypes().Where(t => t.IsAssignableTo(typeof(AgoraService)) && !t.IsAbstract).ToImmutableArray();

                foreach (Type type in types)
                    services.AddAgoraService(type);
            }

            services.AddFusionCache();
            services.AddHttpClient("agora");

            return services;
        }

        public static void AddAgoraService(this IServiceCollection services, Type type)
        {
            var serviceInterfaces = type.GetServiceInterfaces().ToImmutableArray();
            var scope = type.GetCustomAttribute<AgoraServiceAttribute>()?.Scope ?? ServiceLifetime.Singleton;

            services.SetLifetime(scope, type);

            foreach (var serviceType in serviceInterfaces)
                services.SetLifetime(scope, type, serviceType);
        }

        public static IServiceCollection AddEconomyServices(this IServiceCollection services, IConfiguration configuration)
        {
            var unbelievaClientConfig = new UnbelievaClientConfig() { Token = configuration["Token:UnbelievaBoat"] };

            services.AddSingleton(unbelievaClientConfig);
            services.AddSingleton<UnbelievaClient>();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(x => x.AsScoped(), Assembly.GetExecutingAssembly());

            return services;
        }

        private static IServiceCollection SetLifetime(this IServiceCollection services, ServiceLifetime scope, Type serviceType) => scope switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(serviceType),
            ServiceLifetime.Transient => services.AddTransient(serviceType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType),
            _ => services,
        };

        private static IServiceCollection SetLifetime(this IServiceCollection services, ServiceLifetime scope, Type implementationType, Type serviceType) => scope switch
        {
            ServiceLifetime.Singleton => services.AddSingleton(serviceType, implementationType),
            ServiceLifetime.Transient => services.AddTransient(serviceType, implementationType),
            ServiceLifetime.Scoped => services.AddScoped(serviceType, implementationType),
            _ => services,
        };

        private static IEnumerable<Type> GetServiceInterfaces(this Type type)
        {
            if (type.BaseType == null)
                return type.GetInterfaces();
            else
                return type.GetInterfaces().Except(type.BaseType.GetInterfaces());
        }
    }
}
