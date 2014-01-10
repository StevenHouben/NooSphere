using ABC.Infrastructure.Context.Location.Sonitor;
using System;
using System.Collections.Generic;


namespace ABC.Infrastructure.Context.Location
{
    public class LocationTracker : IContextService
    {
        #region Events

        public event TagAddedHandler TagAdded = delegate { };
        public event TagStateChangedHandler TagStateChanged = delegate { };
        public event TagMovedHandler TagMoved = delegate { };

        public event DetectorAddedHandler DetectorAdded = delegate { };
        public event DetectorStateChangedHandler DetectorStateChanged = delegate { };

        public event TagEnterHandler TagEnter = delegate { };
        public event TagLeaveHandler TagLeave = delegate { };


        public event DetectionHandler Detection = delegate { };

        public event TagBatteryHandler TagBatteryDataReceived = delegate { };
        public event TagButtonHandler TagButtonDataReceived = delegate { };

        public event DataReceivedHandler DataReceived = delegate { };

        #endregion


        #region Properties

        public Dictionary<string, Tag> Tags { get; private set; }
        public Dictionary<string, Detector> Detectors { get; private set; }

        public bool IsRunning
        {
            get { return _tracker != null && _tracker.IsRunning; }
        }

        #endregion

        const int DefaultSonitorPort = 43210;

        #region Members

        readonly SonitorTracker _tracker;

        #endregion


        #region Constructor

        public LocationTracker(string address)
        {
            _tracker = new SonitorTracker(address, DefaultSonitorPort);
            Tags = new Dictionary<string, Tag>();
            Detectors = new Dictionary<string, Detector>();

            _tracker.DetectionsReceived += tracker_DetectionsReceived;
            _tracker.DetectorsReceived += tracker_DetectorsReceived;
            _tracker.DetectorStatusReceived += tracker_DetectorStatusReceived;
            _tracker.MapsReceived += tracker_MapsReceived;
            _tracker.ProtocolReceived += tracker_ProtocolReceived;
            _tracker.TagsReceived += tracker_TagsReceived;

            Name = "LocationTracker";
            Id = Guid.NewGuid();
        }

        #endregion


        #region IContextService

        public void Start()
        {
            if ( !_tracker.IsRunning )
                _tracker.Start();
        }

        public void Stop()
        {
            if ( _tracker.IsRunning )
                _tracker.Stop();
        }

        public string Name { get; set; }
        public Guid Id { get; set; }

        public void Send( string message )
        {
            _tracker.Send( message );
        }

        #endregion


        #region Event Handlers

        void tracker_TagsReceived( object sender, SonitorEventArgs e )
        {
            var msg = (TagsMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );
            foreach ( var tag in msg.Tags )
            {
                if ( !Tags.ContainsKey( tag.Id ) )
                {
                    Tags.Add( tag.Id, tag );
                    TagAdded( null, new TagEventArgs( tag ) );
                }
                else
                {
                    Tags[ tag.Id ] = tag;
                    TagStateChanged( null, new TagEventArgs( tag ) );
                }
            }
        }

        void tracker_ProtocolReceived( object sender, SonitorEventArgs e )
        {
            var msg = (ProtocolVersionMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );
        }

        void tracker_MapsReceived( object sender, SonitorEventArgs e )
        {
            var msg = (MapsMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );
        }

        void tracker_DetectorStatusReceived( object sender, SonitorEventArgs e )
        {
            var msg = (DetectorStatusMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );

            foreach ( var state in msg.DetectorStates )
            {
                Detectors[ state.HostName ].Channel = state.Channel;
                Detectors[ state.HostName ].Status = ( state.Online ? OperationStatus.Online : OperationStatus.Offline );
                DetectorStateChanged( this, new DetectorEventArgs( Detectors[ state.HostName ] ) );
            }
        }

        void tracker_DetectorsReceived( object sender, SonitorEventArgs e )
        {
            var msg = (DetectorsMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );

            foreach ( var det in msg.Detectors )
            {
                if ( !Detectors.ContainsKey( det.HostName ) )
                {
                    Detectors.Add( det.HostName, det );
                    DetectorAdded( this, new DetectorEventArgs( Detectors[ det.HostName ] ) );
                }
                else
                {
                    Detectors[ det.HostName ] = det;
                    DetectorStateChanged( this, new DetectorEventArgs( Detectors[ det.HostName ] ) );
                }
            }
        }

        void tracker_DetectionsReceived( object sender, SonitorEventArgs e )
        {
            var msg = (DetectionsMessage)e.Message;
            DataReceived( this, new DataEventArgs( msg ) );

            foreach ( var detection in msg.Detections )
            {
                CheckDetectorChanges( detection );
                CheckBatteryData( detection );
                CheckTagButtonData( detection );
                CheckTagMove( detection );

                Detection( Detectors[ detection.HostName ],
                           new DetectionEventArgs
                           {
                               Aplitude = detection.Amplitude,
                               Confidence = detection.ConfidenceLevel,
                               Detector = Detectors[ detection.HostName ],
                               Tag = Tags[ detection.TagId ],
                               TimeStamp = detection.DateTime
                           } );
            }
        }

        void CheckTagMove( Detection detection )
        {
            if ( Tags[ detection.TagId ].MovingStatus != detection.MovingStatus )
            {
                Tags[ detection.TagId ].MovingStatus = detection.MovingStatus;
                TagMoved( Detectors[ detection.HostName ], new TagEventArgs( Tags[ detection.TagId ] ) );
            }
        }

        void CheckTagButtonData( Detection detection )
        {
            if ( Tags[ detection.TagId ].ButtonA != detection.ButtonAState ||
                 Tags[ detection.TagId ].ButtonB != detection.ButtonBState ||
                 Tags[ detection.TagId ].ButtonC != detection.ButtonCState ||
                 Tags[ detection.TagId ].ButtonD != detection.ButtonDState )
            {
                Tags[ detection.TagId ].ButtonA = detection.ButtonAState;
                Tags[ detection.TagId ].ButtonB = detection.ButtonBState;
                Tags[ detection.TagId ].ButtonC = detection.ButtonCState;
                Tags[ detection.TagId ].ButtonD = detection.ButtonDState;

                TagButtonDataReceived( Tags[ detection.TagId ], new TagEventArgs( Tags[ detection.TagId ] ) );
            }
        }

        void CheckBatteryData( Detection detection )
        {
            if ( Tags[ detection.TagId ].BatteryStatus != detection.BatteryStatus )
            {
                Tags[ detection.TagId ].BatteryStatus = detection.BatteryStatus;
                TagBatteryDataReceived( Tags[ detection.TagId ], new TagEventArgs( Tags[ detection.TagId ] ) );
            }
        }

        void CheckDetectorChanges( Detection detection )
        {
            if ( Tags[ detection.TagId ].Detector != null )
            {
                if ( detection.HostName != Tags[ detection.TagId ].Detector.HostName )
                {
                    Tags[ detection.TagId ].Detector.DetachTag( Tags[ detection.TagId ] );
                    TagLeave( Tags[ detection.TagId ].Detector, new TagEventArgs( Tags[ detection.TagId ] ) );

                    Detectors[ detection.HostName ].AttachTag( Tags[ detection.TagId ] );
                    Tags[ detection.TagId ].Detector = Detectors[ detection.HostName ];
                    TagEnter( Detectors[ detection.HostName ], new TagEventArgs( Tags[ detection.TagId ] ) );
                }
            }
            else
            {
                Detectors[ detection.HostName ].AttachTag( Tags[ detection.TagId ] );
                Tags[ detection.TagId ].Detector = Detectors[ detection.HostName ];
                TagEnter( Detectors[ detection.HostName ], new TagEventArgs( Tags[ detection.TagId ] ) );
            }
        }

        #endregion
    }
}