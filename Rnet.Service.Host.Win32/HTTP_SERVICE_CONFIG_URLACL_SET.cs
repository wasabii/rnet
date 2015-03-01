using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_URLACL_SET
    {

        public HTTP_SERVICE_CONFIG_URLACL_KEY KeyDesc;

        public HTTP_SERVICE_CONFIG_URLACL_PARAM ParamDesc;

    }

}
