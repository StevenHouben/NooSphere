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
using System.Runtime.InteropServices;
using System.Windows.Forms;
using NooSphere.Context.Multicast;
using Microsoft.Win32;

namespace NooSphere.Context.IO
{
    public class PointerNode:IContextService
    {
        private MulticastSocket _mSocket;
        private MouseHook _mouseHook;
        public PointerRole PointerRole { get; private set; }

        public PointerNode(PointerRole role)
        {
            PointerRole = role;
            _mouseHook = new MouseHook();
            _mSocket = new MulticastSocket("225.5.6.7", 5000, 10);
            _mSocket.OnNotifyMulticastSocketListener += _mSocket_OnNotifyMulticastSocketListener;

            Initialize(PointerRole);
        }

        private void Initialize(PointerRole role)
        {
            switch(role)
            {
                case PointerRole.Controller:
                    _mouseHook.Install();
                    _mouseHook.MouseDown += MouseHookMouseDown;
                    _mouseHook.MouseUp += _mouseHook_MouseUp;
                    _mouseHook.MouseMove += _mouseHook_MouseMove;
                    break;
                case PointerRole.Slave:
                    _mSocket.StartReceiving();
                    break;
                default:
                     _mouseHook.Install();
                    _mouseHook.MouseDown += MouseHookMouseDown;
                    _mouseHook.MouseUp += _mouseHook_MouseUp;
                    _mouseHook.MouseMove += _mouseHook_MouseMove;
                    _mSocket.StartReceiving();
                    break;
            }
        }

        void _mouseHook_MouseMove(object sender, MouseHookEventArgs e)
        {
            Send(new PointerMessage(e.X, e.Y, PointerEvent.Move).ToString());
        }

        void _mouseHook_MouseUp(object sender, MouseHookEventArgs e)
        {
            Send(new PointerMessage(e.X, e.Y, PointerEvent.Up).ToString());
        }

        void MouseHookMouseDown(object sender, MouseHookEventArgs e)
        {
            Send(new PointerMessage(e.X, e.Y, PointerEvent.Down).ToString());
        }


        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int x, int y);
        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;
        void _mSocket_OnNotifyMulticastSocketListener(object sender, NotifyMulticastSocketListenerEventArgs e)
        {
            if (e.Type == MulticastSocketMessageType.MessageReceived)
            {
                var msg = System.Text.Encoding.ASCII.GetString((byte[])e.NewObject);

                var res = new PointerMessage(Convert.ToInt32(msg.Split('@')[0]), Convert.ToInt32(msg.Split('@')[1]), (PointerEvent)Enum.Parse(typeof(PointerEvent), msg.Split('@')[2]));
                
                Console.WriteLine(msg);

                HandleMessage(res);
                //if(DataReceived != null)
                //{
                //    DataReceived(this,new DataEventArgs(msg));
                //}
            }
        }

        private void HandleMessage(PointerMessage res)
        {
            switch (res.Message)
            {
                case PointerEvent.Move:
                    SetCursorPos(res.X, res.Y);
                    break;
                case PointerEvent.Down:
                    mouse_event(MOUSEEVENTF_LEFTDOWN, res.X, res.Y, 0, 0);
                    break;
                case PointerEvent.Up:
                    mouse_event(MOUSEEVENTF_LEFTUP, res.X, res.Y, 0, 0);
                    break;
            }
        }


        public string Name { get; set; }

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
            return X + "@" + Y + "@"+Message;
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
        Move,
        Up,
        Down
    }


}
