using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.ActivityBase;

namespace NooSphere.Infrastructure.Web.Controllers
{
    public class MessagesController: ApiController
    {
        readonly ActivityService _service;


        public MessagesController(ActivityService service)
        {
            _service = service;
        }

        public void Post(JObject message)
        {
            
        }
    }
}
