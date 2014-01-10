using ABC.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;


namespace ABC.Infrastructure.Files
{
    public interface IFileServer
    {
        [OperationContract]
        [WebInvoke( RequestFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "POST" )]
        void AddFile( FileRequest file );

        [OperationContract]
        [WebGet( UriTemplate = "files/{activityId}/{resourceId}" )]
        Stream GetFile( string activityId, string resourceId );

        [OperationContract]
        [WebGet( UriTemplate = "files" )]
        Stream GetTestFile();

        [OperationContract]
        [WebInvoke( RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "DELETE" )]
        void RemoveFile( Resource resource );

        [OperationContract]
        [WebInvoke( UriTemplate = "files/{activityId}/{resourceId}", Method = "PUT" )]
        void UpdateFile( string activityId, string resourceId, Stream stream );
    }
}