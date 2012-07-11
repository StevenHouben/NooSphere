using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.Core.ContextModel.ComponentModel
{
    public class ILifecycle
    {
        bool Suspendable { get; set; }
        bool Resumable { get; set; }
        bool Shareable { get; set; }
        bool Roameable { get; set; }
        bool Externalizeable { get; set; }
    }
}
