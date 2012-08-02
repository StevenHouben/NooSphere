using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.FileServer;

namespace NooSphere.ActivitySystem.Interfaces.FileService
{
    [ServiceContract]
    public interface IFileServer
    {
        /// <summary>
        /// Because the internal json converter incorrectly converts byte arrays
        /// </summary>
        /// <param name="json"></param>
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "POST")]
        void AddFile(string json);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "DELETE")]
        void RemoveFile(Resource resource);

        [OperationContract]
        [WebInvoke(BodyStyle = WebMessageBodyStyle.WrappedRequest, RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "PUT")]
        void UpdateFile(Resource resource, byte[] file);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "files")]
        List<Resource> Sync();
    }
}
