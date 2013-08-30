using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Threading.Tasks;

namespace Rnet.Service
{

    [ServiceContract]
    interface IRnetService
    {

        [OperationContract]
        [WebGet(UriTemplate = "/devices", ResponseFormat = WebMessageFormat.Json)]
        BusInfo GetBus();

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}", ResponseFormat = WebMessageFormat.Json)]
        BusControllerInfo GetController(string controllerId);

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/profiles", ResponseFormat = WebMessageFormat.Json)]
        Task<List<ProfileRef>> GetControllerProfiles(string controllerId);

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/profiles/{profileName}", ResponseFormat = WebMessageFormat.Json)]
        ProfileInfo GetControllerProfile(string controllerId, string profileName);

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/{zoneId}", ResponseFormat = WebMessageFormat.Json)]
        BusZoneInfo GetZone(string controllerId, string zoneId);

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/{zoneId}/{deviceId}", ResponseFormat = WebMessageFormat.Json)]
        BusDeviceInfo GetDevice(string controllerId, string zoneId, string deviceId);

        [OperationContract(AsyncPattern=true)]
        [WebGet(UriTemplate = "/devices/{controllerId}/{zoneId}/{deviceId}/profiles", ResponseFormat = WebMessageFormat.Json)]
        Task<List<ProfileRef>> GetDeviceProfiles(string controllerId, string zoneId, string deviceId);

        [OperationContract]
        [WebGet(UriTemplate = "/devices/{controllerId}/{zoneId}/{deviceId}/profiles/{profileName}", ResponseFormat = WebMessageFormat.Json)]
        ProfileInfo GetDeviceProfile(string controllerId, string zoneId, string deviceId, string profileName);

    }

}
