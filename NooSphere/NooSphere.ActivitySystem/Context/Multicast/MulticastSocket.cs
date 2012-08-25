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
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;

namespace NooSphere.ActivitySystem.Context.Multicast
{

    /// <summary>
    /// Taken from http://www.osix.net/modules/article/?id=409
    /// Modified by Juan Hincapie Ramos
    /// </summary>
    public class MulticastSocket
    {
        public event NotifyMulticastSocketListener OnNotifyMulticastSocketListener;

        //Socket creation, regular UDP socket 
        private readonly Socket _udpSocket;
        private Int32 _mConsecutive;

        private EndPoint _localEndPoint;
        private IPEndPoint _localIPEndPoint;

        private readonly string _targetIP;
        private readonly int _targetPort;
        private readonly int _udpTtl;

        //socket initialization 
        public MulticastSocket(string tIP, int tPort, int ttl)
        {
            _udpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _mConsecutive = 0;

            _targetIP = tIP;
            _targetPort = tPort;
            _udpTtl = ttl;

            SetupSocket();
        }

        private void SetupSocket()
        {
            if (_udpSocket.IsBound)
                throw new ApplicationException("The socket is already bound and receving.");

            //recieve data from any source 
            _localIPEndPoint = new IPEndPoint(IPAddress.Any, _targetPort);
            _localEndPoint = _localIPEndPoint;

            //init Socket properties:
            _udpSocket.SetSocketOption(SocketOptionLevel.Udp, SocketOptionName.NoDelay, 1);

            //allow for loopback testing 
            _udpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1);

            //extremly important to bind the Socket before joining multicast groups 
            _udpSocket.Bind(_localIPEndPoint);

            //set multicast flags, sending flags - TimeToLive (TTL) 
            // 0 - LAN 
            // 1 - Single Router Hop 
            // 2 - Two Router Hops... 
            _udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _udpTtl);

            //join multicast group 
            _udpSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(IPAddress.Parse(_targetIP)));

            NotifyMulticastSocketListener(MulticastSocketMessageType.SocketStarted, null);
        }

        public void StartReceiving()
        {
            if (OnNotifyMulticastSocketListener == null)
                throw new ApplicationException("No socket listener has been specified at OnNotifyMulticastSocketListener.");

            // Create the state object. 
            var state = new StateObject {WorkSocket = _udpSocket};

            //get in waiting mode for data - always (this doesn't halt code execution) 
            Recieve(state);
        }

        //initial receive function
        private void Recieve(StateObject state)
        {
            // Begin receiving the data from the remote device. 
            var client = state.WorkSocket;
            client.BeginReceiveFrom(state.Buffer, 0, StateObject.BufferSize, 0, ref _localEndPoint, ReceiveCallback, state);
        }

        //executes the asynchronous receive - executed everytime data is received on the port 
        private void ReceiveCallback(IAsyncResult ar)
        {
            // Retrieve the state object and the client socket from the async state object. 
            StateObject state = null;
            try
            {
                state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // Read data from the remote device. 
                var bytesRead = client.EndReceiveFrom(ar, ref _localEndPoint);

                // Makes a copy of the buffer so it can be cleant up and reused while the listeners are notified in parallel threads.
                var bufferCopy = new byte[bytesRead];
                Array.Copy(state.Buffer, 0, bufferCopy, 0, bytesRead);

                // Listeners are notified in a different thread
                NotifyMulticastSocketListener(MulticastSocketMessageType.MessageReceived, bufferCopy, ++_mConsecutive);

                //keep listening 
                for (var i = 0; i < bytesRead; i++)
                    state.Buffer[i] = (byte)'\0';
                Recieve(state);
            }
            catch (Exception e)
            {
                NotifyMulticastSocketListener(MulticastSocketMessageType.ReceiveException, e);
                if (state != null)
                    Recieve(state);
                else
                    StartReceiving();
            }
        }

        //client send function 
        public void Send(string sendData)
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes(sendData);

            //set the target IP 
            var remoteIPEndPoint = new IPEndPoint(IPAddress.Parse(_targetIP), _targetPort);
            var remoteEndPoint = (EndPoint)remoteIPEndPoint;

            //do asynchronous send 
            _udpSocket.BeginSendTo(bytesToSend, 0, bytesToSend.Length, SocketFlags.None, remoteEndPoint, SendCallback, _udpSocket);
        }

        //executes the asynchronous send 
        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object. 
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device. 
                var bytesSent = client.EndSendTo(ar);

                // Notifies sending completed
                NotifyMulticastSocketListener(MulticastSocketMessageType.MessageSent, bytesSent);
            }
            catch (Exception e)
            {
                NotifyMulticastSocketListener(MulticastSocketMessageType.SendException, e);
            }
        }

        private void NotifyMulticastSocketListener(MulticastSocketMessageType messageType, Object obj)
        {
                     Task.Factory.StartNew(ThreadedNotifyMulticastSocketListener, new NotifyMulticastSocketListenerEventArgs(messageType, obj));
        }
        private void NotifyMulticastSocketListener(MulticastSocketMessageType messageType, Object obj, int consecutive)
        {
            Task.Factory.StartNew(ThreadedNotifyMulticastSocketListener, new NotifyMulticastSocketListenerEventArgs(messageType, obj, consecutive));
        }
        private void ThreadedNotifyMulticastSocketListener(Object argsObj)
        {
            try
            {
                if (OnNotifyMulticastSocketListener != null)
                    OnNotifyMulticastSocketListener(this, (NotifyMulticastSocketListenerEventArgs)argsObj);
            }
            catch { }
        }

    }
}
