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
using System.ServiceModel.Web;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Base.Client
{
    [ServiceContract]
    interface IFileNetEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileDownloadRequest", Method = "POST")]
        void FileNetDownloadRequest(Resource resource);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileDeleteRequest", Method = "POST")]
        void FileNetDeleteRequest(Resource resource);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "FileUploadRequest", Method = "POST")]
        void FileNetUploadRequest(Resource resource);
    }
    public enum FileEvent
    {
        FileDownloadRequest,
        FileUploadRequest,
        FileDeleteRequest
    }
}
