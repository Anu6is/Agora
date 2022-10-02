using Emporia.Domain.Common;

namespace Agora.Shared.Extensions
{
    public static class ReferenceCodeExtensions
    {
        public static string Code(this ReferenceCode referenceCode) => referenceCode.Value.Split(':')[0];

        public static ulong Reference(this ReferenceCode referenceCode)
            => referenceCode.Value.Contains(':') ? ulong.Parse(referenceCode.Value.Split(':')[1]) : 0;
    }
}
