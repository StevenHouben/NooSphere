using NooSphere.ActivitySystem.Base;

namespace NooSphere.ActivitySystem.FileServer
{
    public  interface IFileStore
    {
        string BasePath { get; set; }

        event FileAddedHandler FileAdded;
        event FileChangedHandler FileChanged;
        event FileRemovedHandler FileRemoved;
    }
}
