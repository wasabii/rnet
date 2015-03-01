using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential, Pack = 2)]
    public struct HTTPAPI_VERSION
    {

        public ushort HttpApiMajorVersion;
        public ushort HttpApiMinorVersion;

        public HTTPAPI_VERSION(ushort majorVersion, ushort minorVersion)
        {
            HttpApiMajorVersion = majorVersion;
            HttpApiMinorVersion = minorVersion;
        }

    }

}
