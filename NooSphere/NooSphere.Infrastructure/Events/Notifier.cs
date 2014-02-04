using Microsoft.AspNet.SignalR;

namespace NooSphere.Infrastructure.Events
{
    public class Notifier
    {
        public static void NotifyAll(NotificationType type, object obj)
        {
            var context = GlobalHost.ConnectionManager.GetConnectionContext<EventDispatcher>();
            var output = ConstructEvent(type, obj);
            context.Connection.Broadcast(output);

        }

        public static void NotifyConnection(string connection,NotificationType type, object obj)
        {
            var context = GlobalHost.ConnectionManager.GetConnectionContext<EventDispatcher>();
            var output = ConstructEvent(type, obj);
            context.Connection.Send(connection,output);
        }

        protected static object ConstructEvent(NotificationType type, object obj)
        {
            var notevent = new { Event = type.ToString(), Data = obj };
            return notevent;
        }
    }
}

