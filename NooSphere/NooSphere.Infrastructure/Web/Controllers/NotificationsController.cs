using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Model.Notifications;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NooSphere.Infrastructure.Web.Controllers
{
    public class NotificationsController : ApiController
    {
        readonly ActivitySystem _system;

        public NotificationsController(ActivitySystem system)
        {
            _system = system;
        }

        public List<INotification> Get()
        {
            return _system.GetNotifications();
        }

        public INotification Get(string id)
        {
            return _system.GetNotification(id);
        }

        public void Post(JObject Notification)
        {
            _system.AddNotification(Helpers.Json.ConvertFromTypedJson<INotification>(Notification.ToString()));
        }

        public void Delete(string id)
        {
            _system.RemoveNotification(id);
        }

        public void Put(JObject Notification)
        {
            _system.UpdateNotification(Helpers.Json.ConvertFromTypedJson<INotification>(Notification.ToString()));
        }
    }
}
