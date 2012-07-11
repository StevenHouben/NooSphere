using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Contracts.NetEvents
{
    [ServiceContract]
    public interface IFileNetEvent : IEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileAdded", Method = "POST")]
        void FileNetAdded(Resource resource);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileRemoved", Method = "POST")]
        void FileNetRemoved(Resource resource);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileLocked", Method = "POST")]
        void FileNetLocked(Resource resource);
    }
    public enum FileEvent
    {
        FileAdded,
        FileRemoved,
        FileLocked
    }
}
