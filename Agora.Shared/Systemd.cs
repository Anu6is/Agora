using System.Runtime.InteropServices;
using Tmds.DBus;

namespace Agora.Shared
{
    public class Systemd
    {
        private const string Service = "org.freedesktop.systemd1";
        private const string RootPath = "/org/freedesktop/systemd1";

        public static async Task<string> GetServiceStatusAsync(string serviceName)
        {
            using Connection connection = new(Address.System);
            await connection.ConnectAsync();

            var systemd = connection.CreateProxy<ISystemd>(Service, RootPath) as IManager;
            var units = await systemd.ListUnitsAsync();

            var unit = units.FirstOrDefault(u => u.Name.Equals(serviceName, StringComparison.OrdinalIgnoreCase));

            return unit?.ToString() ?? $"Unalble to locate {serviceName}.service";
        }

        [StructLayout(LayoutKind.Sequential)]
        public class Unit
        {
            private readonly string _unitName;
            private readonly string _description;
            private readonly string _loadState;
            private readonly string _activeState;
            private readonly string _subState;
            private readonly string _followUnit;
            private readonly ObjectPath _unitObjectPath;
            private readonly uint _jobId;
            private readonly string _jobType;
            private readonly ObjectPath _jobObjectPath;

            public string Name => _unitName;
            public ObjectPath Path => _unitObjectPath;
            public string ActiveState => _activeState;
            public string Description => _description;

            public override string ToString() => $"{Name} [{ActiveState} - {_subState}]";
        }

        [DBusInterface("org.freedesktop.systemd1.Manager")]
        public interface IManager : IDBusObject { Task<Unit[]> ListUnitsAsync(); }

        [DBusInterface("org.freedesktop.systemd1")]
        public interface ISystemd : IManager { }
    }
}