namespace Agora.Shared.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class AgoraServiceAttribute : Attribute
    {
        public enum ServiceLifetime { Singleton, Transient, Scoped }
        public ServiceLifetime Scope;

        public AgoraServiceAttribute(ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            Scope = lifetime;
        }
    }
}
