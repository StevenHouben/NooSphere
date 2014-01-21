using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ABC.Infrastructure.ActivityBase;
using ABC.Model;
using ABC.Model.Users;
using Raven.Imports.Newtonsoft.Json.Linq;

namespace ABC.Infrastructure.Web.Controllers
{
    public class FilesController : ApiController
    {
        private readonly ActivitySystem _system;

        public FilesController(ActivitySystem system)
        {
            _system = system;
        }

        public HttpResponseMessage Get(Resource resource)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);

            var stream = _system.GetStreamFromResource(resource) as MemoryStream;

            if (stream != null) result.Content = new ByteArrayContent(stream.ToArray());
            result.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");

            return result;
        }

        public void Post(JObject user)
        {
            _system.AddUser(Helpers.Json.ConvertFromTypedJson<IUser>(user.ToString()));
        }

        public void Delete(string id)
        {
            _system.RemoveUser(id);
        }

        public void Put(JObject user)
        {
            _system.UpdateUser(Helpers.Json.ConvertFromTypedJson<IUser>(user.ToString()));
        }
    }
}
