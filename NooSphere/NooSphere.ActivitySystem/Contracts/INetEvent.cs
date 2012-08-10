using System.ServiceModel;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    interface INetEvent : IActivityNetEvent, IDeviceNetEvent, IFileNetEvent, IComNetEvent, IUserEvent
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "")]
        bool Alive();
    }
}
