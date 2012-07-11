using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ContextModel.ComponentModel
{
    public interface ITool:IEntity
    {
        ToolType Type { get; set; }
        ToolState State { get; set; }
    }
    public enum ToolType
    {
        Physical,
        Digital,
        Mediating
    }
    public interface ToolState{}
}
