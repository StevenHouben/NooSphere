using ABC.Infrastructure.Context.Location.Sonitor;
using System.Collections.Generic;


namespace ABC.Infrastructure.Context.Location
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

        public Dictionary<string, Tag> Tags { get; private set; }

        public Detector()
        {
            Tags = new Dictionary<string, Tag>();
        }

        public void AttachTag( Tag t )
        {
            if ( !Tags.ContainsKey( t.Id ) )
            {
                Tags.Add( t.Id, t );
                TagEnter( this, new TagEventArgs( t ) );
            }
        }

        public void DetachTag( Tag t )
        {
            if ( Tags.ContainsKey( t.Id ) )
            {
                Tags.Remove( t.Id );
                TagLeave( this, new TagEventArgs( t ) );
            }
        }
    }
}