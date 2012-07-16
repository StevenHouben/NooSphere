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
using NooSphere.Core.ContextModel;
using NooSphere.Core.Primitives;
using Newtonsoft.Json.Linq;
using System.Web;
using System.Xml;
using System.Security.Principal;
using NooSphere.Helpers;

namespace NooSphere.Core.ActivityModel
{
    public class User : Identity
    {
        #region Constructors
        public User() : base()
        {
        }
        #endregion
        #region Properties
        private object _image;
        public object Image
        {
            get { return _image; }
            set { this._image = value; }
        }
        public string Email { get; set; }
        #endregion
    }
}
