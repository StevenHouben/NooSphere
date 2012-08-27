<h1>NooSphere</h1>
<h2>Description</h2>

NooSphere is an Activity-Based Computing Framework that supports:
- flexible and extendable JSON-based activity model
- distributed multi-user activity management system
- local and cloud-based file distribution and synchronisation
- local and cloud-based event system based on a publish/subscribe mechanism
- flexible client/service model for different configuration

The licence is included in all files:

(c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

Pervasive Interaction Technology Laboratory (pIT lab) - IT University of Copenhagen

This library is free software; you can redistribute it and/or 
modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
as published by the Free Software Foundation. Check 
http://www.gnu.org/licenses/gpl.html for details.

<h2>Installing and running</h2>
- Install Visual Studio 2010 or up
- Install NuGet throught the Visual Studio Extension Manager

- Fork code
- Run the NuGet package restore
- Make sure you have an account at http://activitycloud-1.apphb.com/
- Run application

<h2>Components</h2>
<h3>NooSphere Core Packets</h3>
<em>NooSphere.Core</em>: Core library that represents the activity and all its subcomponents.
<em>NooSphere.Helpers</em>: Helper library for REST calls and serialisation.

<h3>NooSphere Activity System</h3>
<em>NooSphere.ActivitySystem.ActivityService</em>: local activity service and publish/subscribe mechanism.
<em>NooSphere.ActivitySystem.ContextService</em>: under construction.
<em>NooSphere.ActivitySystem.Client</em>: basic activity client used to connect the UI projects to the infrastructure.
<em>NooSphere.ActivitySystem.DiscoveryService</em>: discovery manager and broadcast service that supports dynamic add-hoc aggregation of services and clients
<em>NooSphere.ActivitySystem.Host</em>: generic webhost for activity/context and discovery services.
<em>NooSphere.ActivitySystem.Contracts</em>: interfaces for all services and clients

<h3>NooSphere Platform</h3>
<em>NooSphere.Platform.Windows</em>: win32/shell32/vdm support for integration in Windows.