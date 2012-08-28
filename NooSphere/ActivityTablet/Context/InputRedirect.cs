
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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NooSphere.ActivitySystem.Context;
using NooSphere.ActivitySystem.Context.Multicast;
using NooSphere.Platform.Windows.Hooks;

namespace ActivityTablet.Context
{
    public class InputRedirect : IContextService
    {
        private MulticastSocket _mSocket;
        private Point _previousPoint;
        private bool _unInitializedMouse = true;
        public PointerRole PointerRole { get; private set; }

        public InputRedirect(PointerRole role)
        {
            PointerRole = role;

        }
        public void Start()
        {
            _mSocket = new MulticastSocket("225.5.6.7", 5000, 10);
            _mSocket.OnNotifyMulticastSocketListener += _mSocket_OnNotifyMulticastSocketListener;
            Initialize(PointerRole);
        }
        public void Stop()
        {
            MouseHook.UnRegister();
        }
        private void Initialize(PointerRole role)
        {
            switch (role)
            {
                case PointerRole.Controller:
                    MouseHook.MouseDown += MouseHookMouseDown;
                    MouseHook.MouseMove += MouseHookMouseMove;
                    MouseHook.MouseUp += MouseHookMouseUp;
                    _mSocket.StartReceiving();
                    break;
                case PointerRole.Slave:
                    MouseHook.MouseDown += MouseHookMouseDown;
                    MouseHook.MouseMove += MouseHookMouseMove;
                    MouseHook.MouseUp += MouseHookMouseUp;
                    _mSocket.StartReceiving();
                    break;
            }
        }
        private void MouseHookMouseUp(object sender, MouseEventArgs e)
        {
            if (PointerRole == PointerRole.Controller)
                Send(new PointerMessage(e.X, e.Y, PointerEvent.MouseUp).ToString());
        }
        private void MouseHookMouseMove(object sender, MouseEventArgs e)
        {
            if (_unInitializedMouse)
            {
                _previousPoint = e.Location;
                _unInitializedMouse = false;
            }
            var xDif = _previousPoint.X - e.Location.X;
            var yDif = _previousPoint.Y - e.Location.Y;

            if (PointerRole == PointerRole.Controller)
                Send(new PointerMessage(e.Location.X,e.Location.Y, PointerEvent.MouseMove).ToString());
            _previousPoint = e.Location;
        }
        private void MouseHookMouseDown(object sender, MouseEventArgs e)
        {
            if (PointerRole == PointerRole.Controller)
                Send(new PointerMessage(e.X, e.Y, PointerEvent.MouseDown).ToString());
        }

        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        private void _mSocket_OnNotifyMulticastSocketListener(object sender, NotifyMulticastSocketListenerEventArgs e)
        {
            if (e.Type == MulticastSocketMessageType.MessageReceived)
            {
                var msg = System.Text.Encoding.ASCII.GetString((byte[])e.NewObject);
                var res = new PointerMessage(Convert.ToInt32(msg.Split('@')[0]), Convert.ToInt32(msg.Split('@')[1]), (PointerEvent)Enum.Parse(typeof(PointerEvent), msg.Split('@')[2]));
                HandleMessage(res);
                if (DataReceived != null)
                {
                    DataReceived(this, new DataEventArgs(msg));
                }
            }
        }
        private void HandleMessage(PointerMessage res)
        {
            switch (res.Message)
            {
                case PointerEvent.MouseMove:
                    SetCursorPos(res.X, res.Y);
                    break;
                case PointerEvent.MouseDown:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, res.X, res.Y, 0, 0);
                    break;
                case PointerEvent.MouseUp:
                    mouse_event(MOUSEEVENTF_LEFTUP, res.X, res.Y, 0, 0);
                    break;
            }
        }


        public string Name { get; set; }
        public Guid Id { get; set; }

        public void Send(string message)
        {
            _mSocket.Send(message);
        }

        public event DataReceivedHandler DataReceived;
        public event System.EventHandler Started;
        public event System.EventHandler Stopped;

    }

    public class PointerMessage
    {
        public int X { get; set; }
        public int Y { get; set; }
        public PointerEvent Message { get; set; }

        public PointerMessage(int x, int y, PointerEvent msg)
        {
            X = x;
            Y = y;
            Message = msg;
        }
        public override string ToString()
        {
            return X + "@" + Y + "@" + Message;
        }
    }

    public enum PointerRole
    {
        Controller,
        Slave,
        Both
    }

    public enum PointerEvent
    {
        MouseMove,
        MouseUp,
        MouseDown,
        TouchUp,
        TouchDown
    }


}
