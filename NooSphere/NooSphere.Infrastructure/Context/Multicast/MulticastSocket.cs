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
using System.Diagnostics;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;


namespace ABC.Infrastructure.Context.Multicast
{
    /// <summary>
    /// Taken from http://www.osix.net/modules/article/?id=409
    /// Modified by Juan Hincapie Ramos
    /// </summary>
    public class MulticastSocket
    {
        public event NotifyMulticastSocketListener OnNotifyMulticastSocketListener;

        //Socket creation, regular UDP socket 
        readonly Socket _udpSocket;
        Int32 _mConsecutive;

        EndPoint _localEndPoint;
        IPEndPoint _localIpEndPoint;

        readonly string _targetIp;
        readonly int _targetPort;
        readonly int _udpTtl;

        //socket initialization 
        public MulticastSocket( string tIp, int tPort, int ttl )
        {
            _udpSocket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );
            _mConsecutive = 0;

            _targetIp = tIp;
            _targetPort = tPort;
            _udpTtl = ttl;

            SetupSocket();
        }

        void SetupSocket()
        {
            if ( _udpSocket.IsBound )
                throw new ApplicationException( "The socket is already bound and receving." );

            //recieve data from any source 
            _localIpEndPoint = new IPEndPoint( IPAddress.Any, _targetPort );
            _localEndPoint = _localIpEndPoint;

            //init Socket properties:
            _udpSocket.SetSocketOption( SocketOptionLevel.Udp, SocketOptionName.NoDelay, 1 );

            //allow for loopback testing 
            _udpSocket.SetSocketOption( SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, 1 );

            //extremly important to bind the Socket before joining multicast groups 
            _udpSocket.Bind( _localIpEndPoint );

            //set multicast flags, sending flags - TimeToLive (TTL) 
            // 0 - LAN 
            // 1 - Single Router Hop 
            // 2 - Two Router Hops... 
            _udpSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, _udpTtl );

            //join multicast group 
            _udpSocket.SetSocketOption( SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption( IPAddress.Parse( _targetIp ) ) );

            NotifyMulticastSocketListener( MulticastSocketMessageType.SocketStarted, null );
        }

        public void StartReceiving()
        {
            if ( OnNotifyMulticastSocketListener == null )
                throw new ApplicationException( "No socket listener has been specified at OnNotifyMulticastSocketListener." );

            // Create the state object. 
            var state = new StateObject { WorkSocket = _udpSocket };

            //get in waiting mode for data - always (this doesn't halt code execution) 
            Recieve( state );
        }

        //initial receive function
        void Recieve( StateObject state )
        {
            // Begin receiving the data from the remote device. 
            var client = state.WorkSocket;
            client.BeginReceiveFrom( state.Buffer, 0, StateObject.BufferSize, 0, ref _localEndPoint, ReceiveCallback, state );
        }

        //executes the asynchronous receive - executed everytime data is received on the port 
        void ReceiveCallback( IAsyncResult ar )
        {
            // Retrieve the state object and the client socket from the async state object. 
            StateObject state = null;
            try
            {
                state = (StateObject)ar.AsyncState;
                var client = state.WorkSocket;

                // Read data from the remote device. 
                var bytesRead = client.EndReceiveFrom( ar, ref _localEndPoint );

                // Makes a copy of the buffer so it can be cleant up and reused while the listeners are notified in parallel threads.
                var bufferCopy = new byte[bytesRead];
                Array.Copy( state.Buffer, 0, bufferCopy, 0, bytesRead );

                // Listeners are notified in a different thread
                NotifyMulticastSocketListener( MulticastSocketMessageType.MessageReceived, bufferCopy, ++_mConsecutive );

                //keep listening 
                for ( var i = 0; i < bytesRead; i++ )
                    state.Buffer[ i ] = (byte)'\0';
                Recieve( state );
            }
            catch ( Exception e )
            {
                NotifyMulticastSocketListener( MulticastSocketMessageType.ReceiveException, e );
                if ( state != null )
                    Recieve( state );
                else
                    StartReceiving();
            }
        }

        //client send function 
        public void Send( string sendData )
        {
            byte[] bytesToSend = Encoding.ASCII.GetBytes( sendData );

            //set the target IP 
            var remoteIpEndPoint = new IPEndPoint( IPAddress.Parse( _targetIp ), _targetPort );
            var remoteEndPoint = (EndPoint)remoteIpEndPoint;

            //do asynchronous send 
            _udpSocket.BeginSendTo( bytesToSend, 0, bytesToSend.Length, SocketFlags.None, remoteEndPoint, SendCallback, _udpSocket );
        }

        //executes the asynchronous send 
        void SendCallback( IAsyncResult ar )
        {
            try
            {
                // Retrieve the socket from the state object. 
                var client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device. 
                var bytesSent = client.EndSendTo( ar );

                // Notifies sending completed
                NotifyMulticastSocketListener( MulticastSocketMessageType.MessageSent, bytesSent );
            }
            catch ( Exception e )
            {
                NotifyMulticastSocketListener( MulticastSocketMessageType.SendException, e );
            }
        }

        void NotifyMulticastSocketListener( MulticastSocketMessageType messageType, Object obj )
        {
            Task.Factory.StartNew( ThreadedNotifyMulticastSocketListener, new NotifyMulticastSocketListenerEventArgs( messageType, obj ) );
        }

        void NotifyMulticastSocketListener( MulticastSocketMessageType messageType, Object obj, int consecutive )
        {
            Task.Factory.StartNew( ThreadedNotifyMulticastSocketListener, new NotifyMulticastSocketListenerEventArgs( messageType, obj, consecutive ) );
        }

        void ThreadedNotifyMulticastSocketListener( Object argsObj )
        {
            try
            {
                if ( OnNotifyMulticastSocketListener != null )
                    OnNotifyMulticastSocketListener( this, (NotifyMulticastSocketListenerEventArgs)argsObj );
            }
            catch ( Exception ex )
            {
                Debug.WriteLine( ex.ToString() );
            }
        }
    }
}