using ABC.Infrastructure.Events;
using ABC.Model.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace ABC.Infrastructure.Web.Controllers
{
    public class TestController : ApiController
    {
        public User Get()
        {
            Notifier.NotifyAll(NotificationType.Message, new User());
            return new User();
        }
    }
}
