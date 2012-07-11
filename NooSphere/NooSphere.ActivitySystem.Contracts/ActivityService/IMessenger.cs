using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    public interface IMessenger
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest,RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "messages", Method = "POST")]
        void SendMessage(string id, string message);
    }
}
