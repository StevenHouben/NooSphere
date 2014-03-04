using NooSphere.Infrastructure.Context.Location.Sonitor;
using System.Collections.Concurrent;
using System.Collections.Generic;


namespace NooSphere.Infrastructure.Context.Location
{
    public class Detector
    {
        public string HostName { get; set; }
        public int Channel { get; set; }
        public string Name { get; set; }
        public GenericLocation<float> Location { get; set; }
        public int FloorPlan { get; set; }
        public float Radius { get; set; }
        public OperationStatus Status { get; set; }

        public event TagEnterHandler TagEnter = delegate { };
        public event TagLeaveHandler TagLeave = delegate { };

        public ConcurrentDictionary<string, Tag> Tags { get; private set; }

        public Detector()
        {
            Tags = new ConcurrentDictionary<string, Tag>();
        }

        public void AttachTag( Tag t )
        {
            var added = Tags.TryAdd( t.Id, t );
            if (added) TagEnter( this, new TagEventArgs( t ) );
        }

        public void DetachTag( Tag t )
        {
            Tag removedItem;
            var removed = Tags.TryRemove( t.Id, out removedItem );
            if (removed) TagLeave( this, new TagEventArgs( removedItem ) );
        }
    }
}