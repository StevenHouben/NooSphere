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
using System.ServiceModel;
using System.ServiceModel.Web;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base.Client
{
    [ServiceContract]
    interface IUserEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FriendAdded", Method = "POST")]
        void FriendNetAdded(User u);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FriendRequest", Method = "POST")]
        void FriendNetRequest(User u);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FriendRemoved", Method = "POST")]
        void FriendNetRemoved(Guid i);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ParticipantAdded", Method = "POST")]
        void ParticipantNetAdded(User u, Guid activityId);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "ParticipantRemoved", Method = "POST")]
        void ParticipantNetRemoved(User u, Guid activityId);

    }
    public enum UserEvents
    {
        FriendAdded,
        FriendRequest,
        FriendRemoved,
        ParticipantAdded,
        ParticipantRemoved
    }
}
