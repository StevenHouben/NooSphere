using NooSphere.Model.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NooSphere.Infrastructure.ActivityBase
{
        public class NotificationEventArgs
        {
            public INotification Notification { get; set; }
            public NotificationEventArgs() { }

            public NotificationEventArgs(INotification notification)
            {
                Notification = notification;
            }
        }

        public class NotificationRemovedEventArgs
        {
            public string Id { get; set; }
            public NotificationRemovedEventArgs() { }

            public NotificationRemovedEventArgs(string id)
            {
                Id = id;
            }
        }
}
