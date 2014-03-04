using NooSphere.Model.Primitives;

namespace NooSphere.Model.Resources
{
    public class Resource : Noo, IResource
    {
        public string FileType { get; set; }
        public string ActivityId { get; set; }
        public Resource()
		{
            Type = typeof( IResource ).Name;
		}

    }
}
