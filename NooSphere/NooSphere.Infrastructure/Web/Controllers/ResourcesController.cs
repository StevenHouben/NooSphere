using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Model;

namespace NooSphere.Infrastructure.Web.Controllers
{
    public class ResourcesController : ApiController
    {
        private readonly ActivitySystem _system;

        public ResourcesController(ActivitySystem system)
        {
            _system = system;
        }

        public string Get()
        {
            _system.DeleteAllAttachments();
            Console.WriteLine("WARNING- DEBUG CODE ENABLED");
            return "No-files-here";
        }


        public HttpResponseMessage Get(string id)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            var stream = _system.GetStreamFromResource(id) as MemoryStream;

            if (stream != null)
                result.Content = new ByteArrayContent(stream.ToArray());
            return result;
        }

        public async void Post()
        {
            var request = Request.Content as StreamContent;
            var activityId = Request.Headers.GetValues("activityId").First();
            var resourceType = Request.Headers.GetValues("resourceType").First();
            if (request != null)
            {
                var  stream = await request.ReadAsStreamAsync();
                    if (_system.Activities.ContainsKey(activityId))
                    {
                        _system.AddResourceToActivity(_system.Activities[activityId] as Activity, stream, resourceType);

                    }
 
            }
        }

        public void Delete(Resource resource)
        {
            _system.Activities[resource.ActivityId].Resources.Remove(resource);
            _system.DeleteResource(resource);
        }
    }
}
