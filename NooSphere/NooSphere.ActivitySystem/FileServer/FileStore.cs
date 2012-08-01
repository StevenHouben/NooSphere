using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NooSphere.ActivitySystem.FileServer
{
    public class FileStore
    {
        public string Path { get; set; }    //local file server folders
        public Dictionary<string,string> Files = new Dictionary<string,string>();
        public FileStore(string path)
        {
            this.Path = path;
        }
        public void Add(FileStream stream,string name)
        {
            stream.s
        }
    }
}
s