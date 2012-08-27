using System;

namespace NooSphere.ActivitySystem.Context.Multicast
{
    public class UdpPerformanceTest
    {
        private readonly MulticastSocket _mSocket;
        private int _count = 0;
        public UdpPerformanceTest()
        {
            _mSocket = new MulticastSocket("225.5.6.78", 5000, 10);
            _mSocket.OnNotifyMulticastSocketListener += new NotifyMulticastSocketListener(_mSocket_OnNotifyMulticastSocketListener);
            _mSocket.StartReceiving();
        }
        public void Test(int numberOfPackets)
        {
            for (var i = 0; i < numberOfPackets;i++ )
                _mSocket.Send("hello");
        }

        void _mSocket_OnNotifyMulticastSocketListener(object sender, NotifyMulticastSocketListenerEventArgs e)
        {
            if(e.Type == MulticastSocketMessageType.MessageReceived)
                Console.WriteLine(_count++);
        }
    }
}
