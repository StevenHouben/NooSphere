/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.ServiceModel;
using NooSphere.Core.Devices;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Base.Client
{
    [ServiceContract]
    interface IDeviceNetEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeviceAdded", Method = "POST")]
        void DeviceNetAdded(Device dev);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "DeviceRemoved", Method = "POST")]
        void DeviceNetRemoved(string id);

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
