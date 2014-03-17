using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using NooSphere.Model.Users;
using Newtonsoft.Json.Linq;
using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Infrastructure.Events;


namespace NooSphere.Infrastructure.Web.Controllers
{
    public class UsersController : ApiController
    {
        readonly ActivitySystem _system;

        public UsersController(ActivitySystem system)
        {
            _system = system;
        }

        public List<IUser> Get()
        {
            return _system.GetUsers();
        }

        public IUser Get(string id)
        {
            return _system.GetUser(id);
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