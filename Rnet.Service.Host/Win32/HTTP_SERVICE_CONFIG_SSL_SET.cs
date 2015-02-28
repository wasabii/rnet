using System.Runtime.InteropServices;

namespace Rnet.Service.Host.Win32
{

    [StructLayout(LayoutKind.Sequential)]
    struct HTTP_SERVICE_CONFIG_SSL_SET
    {

        public HTTP_SERVICE_CONFIG_SSL_KEY KeyDesc;

        public HTTP_SERVICE_CONFIG_SSL_PARAM ParamDesc;

    }

}
