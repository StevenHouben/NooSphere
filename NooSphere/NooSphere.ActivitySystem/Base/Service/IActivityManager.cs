/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections.Generic;
using System.ServiceModel;
using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;
using System.ServiceModel.Web;

namespace NooSphere.ActivitySystem.Base.Service
{
    [ServiceContract]
    public interface IActivityManager :IServiceBase, IMessenger,IFileServer
    {
        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "POST")]
        void AddActivity(Activity act,string deviceId);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "PUT")]
        void UpdateActivity(Activity act, string deviceId);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities/{id}", Method = "POST")]
        void SwitchActivity(string id,string deviceId);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, 
            ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities", Method = "DELETE")]
        void RemoveActivity(string activityId, string deviceId);

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities")]
        List<Activity> GetActivities();

        [OperationContract]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "activities/{id}")]
        Activity GetActivity(string id);

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebInvoke( RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "devices", Method = "POST")]
        Guid Register(Device device);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, UriTemplate = "devices/{deviceId}", Method = "DELETE")]
        void UnRegister(string deviceId);

        [OperationContract]
        [ServiceKnownType(typeof(string))]
        [WebGet(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "users")]
        List<User> GetUsers();

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "users", Method = "POST")]
        void RequestFriendShip(string email, string deviceId);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "users", Method = "DELETE")]
        void RemoveFriend(Guid friendId, string deviceId);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "users", Method = "PUT")]
        void RespondToFriendRequest(Guid friendId, bool approval, string deviceId);
    }
}
