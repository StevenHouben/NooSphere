using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Interfaces.FileService
{
    [ServiceContract]
    public interface IFileService
    {
        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "")]
        bool Alive();

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "POST")]
        void AddFile(Resource resource,File file,bool sync);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "DELETE")]
        void RemoveFile(Resource resource,bool sync);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "PUT")]
        void UpdateFile(Resource resource, File file, bool sync);

        [OperationContract]
        [WebGet(ResponseFormat = WebMessageFormat.Json, UriTemplate = "files")]
        List<Resource> Sync();
    }
}
