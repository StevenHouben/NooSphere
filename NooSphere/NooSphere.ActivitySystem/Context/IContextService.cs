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

namespace NooSphere.ActivitySystem.Context
{
    public delegate void DataReceivedHandler(Object sender, DataEventArgs e);

    public interface IContextService
    {
        string Name { get; set; }
        Guid Id { get; set; }

        void Send(string message);
        void Start();
        void Stop();

        event DataReceivedHandler DataReceived;
    }
}
