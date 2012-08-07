using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    public interface IUserEvent : IEvent
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
