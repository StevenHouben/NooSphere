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
using System.Windows.Forms;

namespace NooSphere.Platform.Windows.InteroptServices
{
    public class NativeWindowEx:NativeWindow
    {
        public delegate void MessageRecievedEventHandler(ref Message m);
        public event MessageRecievedEventHandler MessageRecieved;
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (MessageRecieved != null)
            {
                MessageRecieved(ref m);
            }
        }
    }
}
