using System.ServiceModel;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    interface INetEvent : IDeviceNetEvent, IActivityNetEvent, IFileNetEvent, IComNetEvent, IUserEvent
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "")]
        bool Alive();
    }
}
