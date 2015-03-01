using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct HTTP_SERVICE_CONFIG_URLACL_KEY
    {

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pUrlPrefix;

        public HTTP_SERVICE_CONFIG_URLACL_KEY(string urlPrefix)
        {
            pUrlPrefix = urlPrefix;
        }

    }

}
