using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.Client.Events
{
    public delegate void ActivityAddedHandler(Object sender, ActivityEventArgs e);
    public delegate void ActivityRemovedHandler(Object sender, ActivityRemovedEventArgs e);
    public delegate void ActivityChangedHandler(Object sender, ActivityEventArgs e);

    public delegate void DeviceRoleChangedHandler(Object sender, DeviceEventArgs e);
    public delegate void DeviceAddedHandler(Object sender, DeviceEventArgs e);
    public delegate void DeviceRemovedHandler(Object sender, DeviceEventArgs e);

    public delegate void FileChangedHandler(Object sender, FileEventArgs e);
    public delegate void FileAddedHandler(Object sender, FileEventArgs e);
    public delegate void FileRemovedHandler(Object sender, FileEventArgs e);

    public delegate void MessageReceivedHandler(Object sender, ComEventArgs e);

    public delegate void ConnectionEstablishedHandler(Object sender,EventArgs e);

}
