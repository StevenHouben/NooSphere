using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NooSphere.Model.Device;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.ActivityBase;


namespace NooSphere.Infrastructure.Web.Controllers
{
    public class DevicesController : ApiController
    {
        readonly ActivitySystem _system;

        public DevicesController(ActivitySystem system)
        {
            _system = system;
        }

        public List<IDevice> Get()
        {
            return _system.Devices.Values.ToList();
        }

        public IDevice Get(string id)
        {
            return _system.Devices[id];
        }

        public void Post(JObject device)
        {
            _system.AddDevice(Helpers.Json.ConvertFromTypedJson<IDevice>(device.ToString()));
        }

        public void Delete(string id)
        {
            _system.RemoveDevice(id);
        }

        public void Put(JObject device)
        {
            _system.UpdateDevice(Helpers.Json.ConvertFromTypedJson<IDevice>(device.ToString()));
        }
    }
}
