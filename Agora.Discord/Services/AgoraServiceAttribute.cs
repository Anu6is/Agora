namespace Agora.Discord.Services
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    internal class AgoraServiceAttribute : Attribute
    {
        public enum ServiceLifetime { Singleton, Transient, Scoped}
        public ServiceLifetime Scope;

        public AgoraServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Scope = lifetime;
        }
    }
}
