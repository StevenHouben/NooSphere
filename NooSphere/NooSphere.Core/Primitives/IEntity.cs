using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
namespace NooSphere.Core.Primitives
{
    public interface IEntity
    {
        Identity Identity { get; set; }
    }
}
