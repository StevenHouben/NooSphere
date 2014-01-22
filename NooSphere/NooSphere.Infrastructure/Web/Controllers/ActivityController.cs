using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Model;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.Events;


namespace NooSphere.Infrastructure.Web.Controllers
{
    public class ActivitiesController : ApiController
    {
        readonly ActivitySystem _system;


        public ActivitiesController(ActivitySystem system)
        {
            _system = system;
        }

        public List<IActivity> Get()
        {
            return _system.Activities.Values.ToList();
        }

        public IActivity Get(string id)
        {
            return _system.Activities[id];
        }

        public void Post(JObject activity)
        {
            _system.AddActivity(Helpers.Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        }

        public void Delete(string id)
        {
            _system.RemoveActivity(id);
        }

        public void Put(JObject activity)
        {
            _system.UpdateActivity(Helpers.Json.ConvertFromTypedJson<IActivity>(activity.ToString()));
        }
    }
}