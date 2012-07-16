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

namespace NooSphere.Core.Events
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
