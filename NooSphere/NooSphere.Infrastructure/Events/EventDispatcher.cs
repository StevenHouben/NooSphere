using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using System;


namespace ABC.Infrastructure.Events
{
    internal class EventDispatcher : PersistentConnection
    {
        protected override Task OnReceived(IRequest request, string connectionId, string data)
        {
            return Connection.Broadcast(data);
        }

        protected override Task OnConnected(IRequest request, string connectionId)
        {
            return Connection.Send(connectionId, "Connected");
        }

        protected override Task OnReconnected(IRequest request, string connectionId)
        {
            return Connection.Send(connectionId, "ReConnected");
        }

        protected override Task OnDisconnected(IRequest request, string connectionId)
        {
            return Connection.Send(connectionId, "DisConnected");
        }
    }
}