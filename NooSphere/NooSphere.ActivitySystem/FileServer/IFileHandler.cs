using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NooSphere.ActivitySystem.FileServer
{
    public interface IFileHandler
    {
        string LocalPath { get; }
    }
}
