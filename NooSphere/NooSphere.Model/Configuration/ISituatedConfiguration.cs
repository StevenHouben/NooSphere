using System.Collections.Generic;
using NooSphere.Model.Device;
using NooSphere.Model.Primitives;

namespace NooSphere.Model.Configuration
{
    public interface ISituatedConfiguration:INoo
    {
        List<IResourceConfiguration> Configurations { get; set; }

        IDevice Device { set; get; }
    }
}
