using System;


namespace ABC.Infrastructure.Context.Multicast
{
    public class UdpPerformanceTest
    {
        readonly MulticastSocket _mSocket;
        int _count;

        public UdpPerformanceTest()
        {
            _mSocket = new MulticastSocket( "225.5.6.78", 5000, 10 );
            _mSocket.OnNotifyMulticastSocketListener += _mSocket_OnNotifyMulticastSocketListener;
            _mSocket.StartReceiving();
        }

        public void Test( int numberOfPackets )
        {
            for ( var i = 0; i < numberOfPackets; i++ )
                _mSocket.Send( "hello" );
        }

        void _mSocket_OnNotifyMulticastSocketListener( object sender, NotifyMulticastSocketListenerEventArgs e )
        {
            if ( e.Type == MulticastSocketMessageType.MessageReceived )
                Console.WriteLine( _count++ );
        }
    }
}