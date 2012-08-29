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
    interface IActivityNetEvent
    {
        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ActivityAdded", Method = "POST")]
        void ActivityNetAdded(Activity act);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ActivityRemoved", Method = "POST")]
        void ActivityNetRemoved(Guid id);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ActivityChanged", Method = "POST")]
        void ActivityNetChanged(Activity act);

        [OperationContract]
        [WebInvoke(RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json,
            UriTemplate = "ActivitySwitched", Method = "POST")]
        void ActivityNetSwitched(Activity act);
    }

    public enum ActivityEvent
    {
        ActivityAdded,
        ActivityRemoved,
        ActivityChanged,
        ActivitySwitched
    }
}