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
