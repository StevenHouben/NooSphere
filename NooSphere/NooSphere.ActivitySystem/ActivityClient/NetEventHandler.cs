/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;
using System.ServiceModel;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Contracts.NetEvents;
using NooSphere.Core.Events;

namespace NooSphere.ActivitySystem.ActivityClient
{
    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single, InstanceContextMode = InstanceContextMode.Single, UseSynchronizationContext = false)]
    public class NetEventHandler : IActivityNetEvent,IDeviceNetEvent,IFileNetEvent,IComNetEvent
    {
        #region Events
        public event ActivityAddedHandler ActivityAdded = null;
        public event ActivityRemovedHandler ActivityRemoved = null;
        public event ActivityChangedHandler ActivityChanged = null;

        public event DeviceAddedHandler DeviceAdded = null;
        public event DeviceRemovedHandler DeviceRemoved = null;
        public event DeviceRoleChangedHandler DeviceRoleChanged = null;

        public event FileAddedHandler FileAdded = null;
        public event FileChangedHandler FileChanged = null;
        public event FileRemovedHandler FileRemoved = null;

        public event MessageReceivedHandler MessageReceived = null;
        #endregion

        #region Net Event handlers
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
        public void FileNetAdded(Resource r)
        {
            if (FileRemoved != null)
                FileRemoved(this, new FileEventArgs(r));
        }
        public void FileNetRemoved(Resource r)
        {
            if (FileAdded != null)
                FileAdded(this, new FileEventArgs(r));
        }
        public void FileNetLocked(Resource r)
        {
            if (FileChanged != null)
                FileChanged(this, new FileEventArgs(r));
        }
        public void DeviceNetAdded(Core.Devices.Device dev)
        {
            if (DeviceAdded != null)
                DeviceAdded(this, new DeviceEventArgs(dev));

        }
        public void DeviceNetRemoved(Core.Devices.Device dev)
        {
            if (DeviceRemoved != null)
                DeviceRemoved(this, new DeviceEventArgs(dev));
        }
        public void DeviceNetRoleChanged(Core.Devices.Device dev)
        {
            if (DeviceRoleChanged != null)
                DeviceRoleChanged(this, new DeviceEventArgs(dev));
        }
        #endregion

        public bool Alive()
        {
            return true;
        }
    }
}
