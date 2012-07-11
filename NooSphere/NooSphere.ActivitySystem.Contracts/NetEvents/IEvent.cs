using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts.NetEvents
{
    [ServiceContract]
    public interface IEvent
    {
        		[OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "")]
        bool Alive();
    }
}
