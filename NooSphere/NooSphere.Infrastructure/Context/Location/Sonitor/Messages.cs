using System.Collections.Generic;


namespace ABC.Infrastructure.Context.Location.Sonitor
{
    public class SonitorMessage
    {
        public string KeyWord { get; set; }
    }

    public class ProtocolVersionMessage : SonitorMessage
    {
        public string Version { get; set; }

        public ProtocolVersionMessage( string version )
        {
            KeyWord = GetType().Name.ToUpper();
            Version = version;
        }
    }

    public class DetectorsMessage : SonitorMessage
    {
        public List<Detector> Detectors { get; set; }

        public DetectorsMessage()
        {
            KeyWord = GetType().Name.ToUpper();
            Detectors = new List<Detector>();
        }
    }

    public class DetectorStatusMessage : SonitorMessage
    {
        public List<DetectorStatus> DetectorStates { get; set; }

        public DetectorStatusMessage()
        {
            KeyWord = GetType().Name.ToUpper();
            DetectorStates = new List<DetectorStatus>();
        }
    }

    public class TagsMessage : SonitorMessage
    {
        public List<Tag> Tags { get; set; }

        public TagsMessage()
        {
            KeyWord = GetType().Name.ToUpper();
            Tags = new List<Tag>();
        }
    }

    public class MapsMessage : SonitorMessage
    {
        public List<Map> Maps { get; set; }

        public MapsMessage()
        {
            KeyWord = GetType().Name.ToUpper();
            Maps = new List<Map>();
        }
    }

    public class DetectionsMessage : SonitorMessage
    {
        public List<Detection> Detections { get; set; }

        public DetectionsMessage()
        {
            KeyWord = GetType().Name.ToUpper();
            Detections = new List<Detection>();
        }
    }

    public enum SonitorMessages
    {
        Protocolversion,
        Detectors,
        Detectorstatus,
        Tags,
        Maps,
        Detection
    }
}