using System;
using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential)]
    public struct HTTP_SERVICE_CONFIG_IP_LISTEN_PARAM
    {

        public ushort AddrLength;

        public IntPtr pAddress;

    }

}
