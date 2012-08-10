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

using NooSphere.Core.ActivityModel;
using System.ServiceModel;
using NooSphere.ActivitySystem.Contracts;

namespace NooSphere.ActivitySystem.Base
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class NetEventHandler : INetEvent
    {
        #region Events
        public event ActivityAddedHandler ActivityAdded = null;
        public event ActivityRemovedHandler ActivityRemoved = null;
        public event ActivityChangedHandler ActivityChanged = null;

        public event DeviceAddedHandler DeviceAdded = null;
        public event DeviceRemovedHandler DeviceRemoved = null;
        public event DeviceRoleChangedHandler DeviceRoleChanged = null;

        public event MessageReceivedHandler MessageReceived = null;

        public event FileDownloadRequestHandler FileDownloadRequest;
        public event FileUploadRequestHandler FileUploadRequest;
        public event FileDeleteRequestHandler FileDeleteRequest;

        public event FriendAddedHandler FriendAdded;
        public event FriendDeletedHandler FriendDeleted;
        public event FriendRequestReceivedHandler FriendRequestReceived;

        public event ParticipantAddedHandler ParticipantAdded;
        public event ParticipantRemovedHandler ParticipantRemoved;

        public event EventHandler UserOnline;
        public event EventHandler UserOffline;

        #endregion

        #region Net Event handlers
        protected void OnUserOffline(EventArgs e)
        {
            if (UserOffline != null) 
                UserOffline(this, e);
        }

        protected void OnUserOnline(EventArgs e)
        {
            if (UserOnline != null) 
                UserOnline(this, e);
        }
        public void ActivityNetAdded(Activity act)
        {
            if (ActivityAdded != null)
                ActivityAdded(this, new ActivityEventArgs(act));
        }
        public void ActivityNetRemoved(Guid id)
        {
             if (ActivityRemoved != null)
                ActivityRemoved(this, new ActivityRemovedEventArgs(id));
        }
        public void ActivityNetChanged(Activity act)
        {
             if (ActivityChanged != null)
                ActivityChanged(this, new ActivityEventArgs(act));
        }
        public void MessageNetReceived(string msg)
        {
            if (MessageReceived != null)
                MessageReceived(this, new ComEventArgs(msg));
        }     
        public void FileNetDownloadRequest(Resource r)
        {
            if (FileDownloadRequest != null)
                FileDownloadRequest(this, new FileEventArgs(r));
        }
        public void FileNetDeleteRequest(Resource r)
        {
            if (FileDeleteRequest != null)
                FileDeleteRequest(this, new FileEventArgs(r));
        }
        public void FileNetUploadRequest(Resource r)
        {
            if (FileUploadRequest != null)
                FileUploadRequest(this, new FileEventArgs(r));
        }
        public void DeviceNetAdded(Core.Devices.Device dev)
        {
            if (DeviceAdded != null)
                DeviceAdded(this, new DeviceEventArgs(dev));

        }
        public void DeviceNetRemoved(string id)
        {
            if (DeviceRemoved != null)
                DeviceRemoved(this, new DeviceRemovedEventArgs(id));
        }
        public void DeviceNetRoleChanged(Core.Devices.Device dev)
        {
            if (DeviceRoleChanged != null)
                DeviceRoleChanged(this, new DeviceEventArgs(dev));
        }
        public void FriendNetAdded(User u)
        {
            if(FriendAdded != null)
                FriendAdded(this,new FriendEventArgs(u));
        }

        public void FriendNetRequest(User u)
        {
            if(FriendRequestReceived != null)
                FriendRequestReceived(this, new FriendEventArgs(u));
        }

        public void FriendNetRemoved(Guid i)
        {
            if (FriendDeleted != null)
                FriendDeleted(this, new FriendDeletedEventArgs(i));
        }

        public void ParticipantNetAdded(User u, Guid activityId)
        {
            if(ParticipantAdded != null)
                ParticipantAdded(this, new ParticipantEventArgs(u,activityId));
        }

        public void ParticipantNetRemoved(User u, Guid activityId)
        {
            if (ParticipantRemoved != null)
                ParticipantRemoved(this, new ParticipantEventArgs(u, activityId));
        }

        public bool Alive()
        {
            return true;
        }
        #endregion
    }
}
