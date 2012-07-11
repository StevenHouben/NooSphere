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
