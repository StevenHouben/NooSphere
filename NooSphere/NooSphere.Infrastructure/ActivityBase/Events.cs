﻿using System;


namespace NooSphere.Infrastructure.ActivityBase
{
    /// <summary>
    /// Events used to distributed activity model
    /// </summary>
    public delegate void ActivityAddedHandler( Object sender, ActivityEventArgs e );

    public delegate void ActivityRemovedHandler( Object sender, ActivityRemovedEventArgs e );

    public delegate void ActivityChangedHandler( Object sender, ActivityEventArgs e );

    public delegate void ActivitySwitchedHandler( Object sender, ActivityEventArgs e );

    /// <summary>
    /// Participant event
    /// </summary>
    public delegate void UserAddedHandler( Object sender, UserEventArgs e );

    public delegate void UserRemovedHandler( Object sender, UserRemovedEventArgs e );

    public delegate void UserChangedHandler( Object sender, UserEventArgs e );

    public delegate void TcpDataReceivedHandler( Object sender, NetEventArgs e );

    /// <summary>
    /// Connection event
    /// </summary>>
    public delegate void ConnectionEstablishedHandler( Object sender, EventArgs e );

    /// <summary>
    /// Intialize event
    /// </summary>
    public delegate void InitializedHandler( Object sender, EventArgs e );

    /// <summary>
    /// Device events
    /// </summary>
    public delegate void DeviceChangedHandler( Object sender, DeviceEventArgs e );

    public delegate void DeviceAddedHandler( Object sender, DeviceEventArgs e );

    public delegate void DeviceRemovedHandler( Object sender, DeviceRemovedEventArgs e );

    /// <summary>
    /// FileResource events
    /// </summary>

    public delegate void FileResourceAddedHandler(object sender, FileResourceEventArgs e);

    public delegate void FileResourceChangedHandler(object sender, FileResourceEventArgs e);

    public delegate void FileResourceRemovedHandler(object sender, FileResourceRemovedEventArgs e);

    /// <summary>
    /// Resource events
    /// </summary>
    public delegate void ResourceAddedHandler(Object sender, ResourceEventArgs e);

    public delegate void ResourceRemovedHandler(Object sender, ResourceRemovedEventArgs e);

    public delegate void ResourceChangedHandler(Object sender, ResourceEventArgs e);

    /// <summary>
    /// Notification events
    /// </summary>
    public delegate void NotificationAddedHandler(Object sender, NotificationEventArgs e);

    public delegate void NotificationRemovedHandler(Object sender, NotificationRemovedEventArgs e);

    public delegate void NotificationChangedHandler(Object sender, NotificationEventArgs e);

	public delegate void MessageReceivedHandler(object sender, MessageEventArgs e);
}
