/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

namespace NooSphere.ActivitySystem.Base.Controller
{
    public enum Configuration
    {
        LocalActivityStore,
        Client,
        ManagerLocal,
        ManagerCloud,
        ClientAndLocalManagerLocal,
        ClientAndLocalManagerCloud,
        ClientAndHostedManagerLocal,
        ClientAndHostedManagerCloud
    }
}
