using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct HTTP_SERVICE_CONFIG_URLACL_PARAM
    {

        [MarshalAs(UnmanagedType.LPWStr)]
        public string pStringSecurityDescriptor;

        public HTTP_SERVICE_CONFIG_URLACL_PARAM(string securityDescriptor)
        {
            pStringSecurityDescriptor = securityDescriptor;
        }

    }

}
