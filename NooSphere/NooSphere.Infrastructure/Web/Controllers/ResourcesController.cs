using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NooSphere.Model.Resources;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Infrastructure.Events;


namespace NooSphere.Infrastructure.Web.Controllers
{
    public class ResourcesController : ApiController
    {
        readonly ActivitySystem _system;

        public ResourcesController(ActivitySystem system)
        {
            _system = system;
        }

        public List<IResource> Get()
        {
            return _system.GetResources();
        }

        public IResource Get(string id)
        {
            return _system.GetResource(id);
        }

        public void Post(JObject resource)
        {
            _system.AddResource(Helpers.Json.ConvertFromTypedJson<IResource>(resource.ToString()));
        }

        public void Delete(string id)
        {
            _system.RemoveResource(id);
        }

        public void Put(JObject resource)
        {
            _system.UpdateResource(Helpers.Json.ConvertFromTypedJson<IResource>(resource.ToString()));
        }
    }
}
