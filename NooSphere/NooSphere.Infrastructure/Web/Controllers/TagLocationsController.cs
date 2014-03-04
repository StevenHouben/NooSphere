using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Infrastructure.Context.Location;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace NooSphere.Infrastructure.Web.Controllers
{
    public class TagLocationsController : ApiController
    {
        readonly ActivitySystem _system;

        public TagLocationsController(ActivitySystem system)
        {
            _system = system;
        }
        public string Get(string id)
        {
            return _system.Tracker.Tags[id].Detector.Name;
        }
    }
}
