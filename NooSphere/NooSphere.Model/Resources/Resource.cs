using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NooSphere.Model.Primitives;

namespace NooSphere.Model.Resources
{
    public class Resource : Noo, IResource
    {
        public Resource()
		{
            Type = typeof( IResource ).Name;
		}

    }
}
