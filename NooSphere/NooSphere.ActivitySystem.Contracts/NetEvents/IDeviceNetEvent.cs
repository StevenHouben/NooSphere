using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using NooSphere.Core.Devices;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Contracts.NetEvents
{
    [ServiceContract]
    public interface IDeviceNetEvent:IEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeviceAdded", Method = "POST")]
        void DeviceNetAdded(Device dev);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeviceRemoved", Method = "POST")]
        void DeviceNetRemoved(Device dev);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeviceRoleChanged", Method = "POST")]
        void DeviceNetRoleChanged(Device dev);
    }

    public enum DeviceEvent
    {
        DeviceAdded,
        DeviceRemoved,
        DeviceRoleChanged
    }
}
