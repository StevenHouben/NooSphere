using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using System.ServiceModel.Web;
using System.IO;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    public interface IActivityManager:IMessenger
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "POST")]
        void AddActivity(Activity act);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "PUT")]
        void UpdateActivity(Activity act);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "DELETE")]
        void RemoveActivity(string id);

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities")]
        object GetActivities();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities/{id}")]
        object GetActivity(string id);

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "devices", Method = "POST")]
        object Register(Device device);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "devices", Method = "DELETE")]
        void UnRegister(string id);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "subscribers", Method = "POST")]
        void Subscribe(string id, EventType type, int port);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "subscribers", Method = "DELETE")]
        void UnSubscribe(string id,EventType type);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "")]
        bool Alive();
    }
}
