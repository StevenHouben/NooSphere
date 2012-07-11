using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Principal;

namespace NooSphere.Core.ActivityModel
{
    public class Credentials : IPrincipal
    {
        public string Email { get; set; }

        public IIdentity Identity
        {
            get { return new GenericIdentity("Email"); }
        }

        public bool IsInRole(string role)
        {
            return true;
        }
    }
}
