using System;


namespace ABC.Infrastructure.Context.Location
{
    //Initial tag event, called by the central tracker
    public delegate void TagAddedHandler( Detector detector, TagEventArgs e );

    public delegate void TagRemovedHandler( Detector detector, TagEventArgs e );

    public delegate void TagStateChangedHandler( Detector detector, TagEventArgs e );

    //Events called by individual Decectors
    public delegate void TagEnterHandler( Detector detector, TagEventArgs e );

    public delegate void TagLeaveHandler( Detector detector, TagEventArgs e );

    public delegate void TagMovedHandler( Detector detector, TagEventArgs e );

    public delegate void TagBatteryHandler( Tag tag, TagEventArgs e );

    public delegate void TagButtonHandler( Tag tag, TagEventArgs e );

    public delegate void DetectorAddedHandler( Object sender, DetectorEventArgs e );

    public delegate void DetectorRemovedHandler( Object sender, DetectorEventArgs e );

    public delegate void DetectorStateChangedHandler( Object sender, DetectorEventArgs e );

    public delegate void DetectionHandler( Detector detector, DetectionEventArgs e );

    public class TagEventArgs
    {
        public Tag Tag { get; set; }

        public TagEventArgs( Tag tag )
        {
            Tag = tag;
        }
    }

    public class DetectorEventArgs
    {
        public Detector Detector { get; set; }

        public DetectorEventArgs( Detector detector )
        {
            Detector = detector;
        }
    }

    public class DetectionEventArgs
    {
        public Detector Detector { get; set; }
        public Tag Tag { get; set; }
        public DateTime TimeStamp { get; set; }
        public float Aplitude { get; set; }
        public float Confidence { get; set; }
    }
}