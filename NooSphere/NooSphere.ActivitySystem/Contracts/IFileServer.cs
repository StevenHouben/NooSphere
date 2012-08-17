﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Collections.Generic;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.IO;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Contracts
{
    [ServiceContract]
    public interface IFileServer
    {
        [OperationContract]
        [WebInvoke(UriTemplate = "files/{activityId}/{resourceId}", Method = "POST")]
        void AddFile(string activityId,string resourceId, Stream stream);

        [OperationContract]
        [WebGet(UriTemplate = "files/{activityId}/{resourceId}")]
        Stream GetFile(string activityId, string resourceId);

        [OperationContract]
        [WebGet(UriTemplate = "file")]
        Stream GetTestFile();

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json, UriTemplate = "files", Method = "DELETE")]
        void RemoveFile(Resource resource);

        [OperationContract]
        [WebInvoke(UriTemplate = "files/{activityId}/{resourceId}", Method = "PUT")]
        void UpdateFile(string activityId, string resourceId, Stream stream);




    }
}
