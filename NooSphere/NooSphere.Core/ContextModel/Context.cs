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
using NooSphere.Core.ContextModel.ComponentModel;

namespace NooSphere.Core.ContextModel
{
    public class Context
    {
        public ILifecycle LifeCycle { get; set; }
        public List<IView> Views { get; set; }
        public List<IRule> Rules { get; set; }
        public List<ITool> Tools { get; set; }
    }
}
