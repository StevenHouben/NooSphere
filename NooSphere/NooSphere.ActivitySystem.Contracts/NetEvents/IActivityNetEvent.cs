using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts.NetEvents
{
    [ServiceContract]
    public interface IActivityNetEvent:IEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ActivityAdded", Method = "POST")]
        void ActivityNetAdded(Activity act);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ActivityRemoved", Method = "POST")]
        void ActivityNetRemoved(Guid id);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ActivityChanged", Method = "POST")]
        void ActivityNetChanged(Activity act);
    }

    public enum ActivityEvent
    {
        ActivityAdded,
        ActivityRemoved,
        ActivityChanged
    }
}
