/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

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
