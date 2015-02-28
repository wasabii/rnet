using System;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace Rnet.Service.Host.Win32
{

    public static class HttpApi
    {

        const uint NOERROR = 0x00;
        const uint ERROR_ALREADY_EXISTS = 183;
        const uint HTTP_INITIALIZE_CONFIG = 0x00000002;

        static readonly HTTPAPI_VERSION HTTPAPI_VERSION_2 = new HTTPAPI_VERSION(2, 0);

        [DllImport("httpapi.dll", SetLastError = true)]
        public static extern uint HttpInitialize(
            HTTPAPI_VERSION Version,
            uint Flags,
            IntPtr pReserved);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpSetServiceConfiguration(
             IntPtr ServiceIntPtr,
             HTTP_SERVICE_CONFIG_ID ConfigId,
             IntPtr pConfigInformation,
             int ConfigInformationLength,
             IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpDeleteServiceConfiguration(
            IntPtr ServiceIntPtr,
            HTTP_SERVICE_CONFIG_ID ConfigId,
            IntPtr pConfigInformation,
            int ConfigInformationLength,
            IntPtr pOverlapped);

        [DllImport("httpapi.dll", SetLastError = true)]
        static extern uint HttpTerminate(
            uint Flags,
            IntPtr pReserved);

        public static void ReserveURL(string networkURL, SecurityIdentifier sid)
        {
            var retVal = (uint)NOERROR; // NOERROR = 0

            retVal = HttpApi.HttpInitialize(HttpApi.HTTPAPI_VERSION_2, HttpApi.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            if ((uint)NOERROR == retVal)
            {
                var securityDescriptor = string.Format(CultureInfo.InvariantCulture, "D:(A;;GX;;;{0})", sid);
                var keyDesc = new HTTP_SERVICE_CONFIG_URLACL_KEY(networkURL);
                var paramDesc = new HTTP_SERVICE_CONFIG_URLACL_PARAM(securityDescriptor);

                var inputConfigInfoSet = new HTTP_SERVICE_CONFIG_URLACL_SET();
                inputConfigInfoSet.KeyDesc = keyDesc;
                inputConfigInfoSet.ParamDesc = paramDesc;

                var pInputConfigInfo = Marshal.AllocCoTaskMem(Marshal.SizeOf(typeof(HTTP_SERVICE_CONFIG_URLACL_SET)));
                Marshal.StructureToPtr(inputConfigInfoSet, pInputConfigInfo, false);

                retVal = HttpApi.HttpSetServiceConfiguration(IntPtr.Zero,
                    HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                    pInputConfigInfo,
                    Marshal.SizeOf(inputConfigInfoSet),
                    IntPtr.Zero);

                if ((uint)ERROR_ALREADY_EXISTS == retVal) // ERROR_ALREADY_EXISTS = 183
                {
                    retVal = HttpApi.HttpDeleteServiceConfiguration(
                        IntPtr.Zero,
                        HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                        pInputConfigInfo,
                        Marshal.SizeOf(inputConfigInfoSet),
                        IntPtr.Zero);

                    if ((uint)NOERROR == retVal)
                    {
                        retVal = HttpApi.HttpSetServiceConfiguration(IntPtr.Zero,
                            HTTP_SERVICE_CONFIG_ID.HttpServiceConfigUrlAclInfo,
                            pInputConfigInfo,
                            Marshal.SizeOf(inputConfigInfoSet),
                            IntPtr.Zero);
                    }
                }

                Marshal.FreeCoTaskMem(pInputConfigInfo);
                HttpApi.HttpTerminate(HttpApi.HTTP_INITIALIZE_CONFIG, IntPtr.Zero);
            }

            if ((uint)NOERROR != retVal)
            {
                throw new Win32Exception(Convert.ToInt32(retVal));
            }
        }

    }

}
