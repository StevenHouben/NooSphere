using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts.NetEvents
{
    [ServiceContract]
    public interface IComNetEvent : IEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "MessageReceived", Method = "POST")]
        void MessageNetReceived(string msg);
    }
    public enum ComEvent
    {
        MessageReceived
    }
}
