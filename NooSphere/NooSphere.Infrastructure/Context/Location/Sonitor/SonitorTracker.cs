using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace ABC.Infrastructure.Context.Location.Sonitor
{
    public class SonitorTracker : IContextService
    {
        public event SonitorMessageReceivedHandler TagsReceived = delegate { };

        public event DataReceivedHandler DataReceived = delegate { };

        public bool IsRunning
        {
            get { return Running; }
        }

        public string Address { get; private set; }
        public int Port { get; private set; }

        public SonitorTracker(string addr, int port)
        {
            Address = addr;
            Port = port;
        }
        public void Start()
        {
            Running = true;

            Task.Factory.StartNew( RunTcpClient );
        }

        public void Stop()
        {
            Running = false;
            Debug.WriteLine( GetType().Name + " stopped" );
        }

        public static bool Running { get; private set; }

        void RunTcpClient()
        {
            try
            {
                var client = new TcpClient();

                client.Connect( IPAddress.Parse(Address),Port);

                var reader = new StreamReader( client.GetStream(), Encoding.ASCII );

                Debug.WriteLine( GetType().Name + " started" );

                var message = new List<string>();

                while ( Running )
                {
                    var line = reader.ReadLine();
                    if ( line == "" )
                    {
                        ParseRawMessage( message );
                        message.Clear();
                    }
                    else
                        message.Add( line );
                    DataReceived( this, new DataEventArgs( message.ToArray() ) );
                }

                client.Close();
                Console.WriteLine( "Location Tracker closing" );
            }
            catch ( Exception e )
            {
                Console.WriteLine( "Error: " + e );
            }
        }

        void ParseRawMessage( List<string> msg )
        {
            var head = msg[ 0 ];

            switch ( SonitorConverter.DetermineMessage( head ) )
            {
                case SonitorMessages.Detection:
                    HandleDetectionMessage( msg );
                    break;
                case SonitorMessages.Detectors:
                    HandleDetectorsMessage( msg );
                    break;
                case SonitorMessages.Detectorstatus:
                    HandleDetectorStatusMessage( msg );
                    break;
                case SonitorMessages.Maps:
                    HandleMapsMessage( msg );
                    break;
                case SonitorMessages.Protocolversion:
                    HandleProtocolMessage( msg );
                    break;
                case SonitorMessages.Tags:
                    HandleTagsMessage( msg );
                    break;
            }
        }

        void HandleTagsMessage( List<string> msg )
        {
            var message = new TagsMessage();
            for ( int i = 1; i < msg.Count; i++ )
            {
                var rawDetection = msg[ i ].Split( ',' );
                message.Tags.Add(
                    new Tag
                    {
                        Id = rawDetection[ 0 ],
                        Name = rawDetection[ 1 ],
                        ImageUrl = rawDetection[ 2 ]
                    } );
            }
            TagsReceived( this, new SonitorEventArgs( message ) );
        }

        public event SonitorMessageReceivedHandler MapsReceived = delegate { };

        void HandleMapsMessage( List<string> msg )
        {
            var message = new MapsMessage();
            for ( int i = 1; i < msg.Count; i++ )
            {
                var rawDetection = msg[ i ].Split( ',' );
                message.Maps.Add(
                    new Map
                    {
                        FloorNumber = Convert.ToInt16( rawDetection[ 0 ] ),
                        Name = rawDetection[ 1 ],
                        ImageUrl = rawDetection[ 2 ]
                    } );
            }
            MapsReceived( this, new SonitorEventArgs( message ) );
        }

        public event SonitorMessageReceivedHandler DetectorStatusReceived = delegate { };

        void HandleDetectorStatusMessage( List<string> msg )
        {
            var message = new DetectorStatusMessage();
            for ( int i = 1; i < msg.Count; i++ )
            {
                var rawDetection = msg[ i ].Split( ',' );

                message.DetectorStates.Add(
                    new DetectorStatus
                    {
                        HostName = rawDetection[ 0 ],
                        Channel = Convert.ToInt16( rawDetection[ 1 ] ),
                        Online = Convert.ToInt16( rawDetection[ 2 ] ) == 1
                    } );
            }
            DetectorStatusReceived( this, new SonitorEventArgs( message ) );
        }

        public event SonitorMessageReceivedHandler DetectorsReceived = delegate { };

        void HandleDetectorsMessage( List<string> msg )
        {
            var message = new DetectorsMessage();
            for ( int i = 1; i < msg.Count; i++ )
            {
                var rawDetection = msg[ i ].Split( ',' );
                message.Detectors.Add(
                    new Detector
                    {
                        HostName = rawDetection[ 0 ],
                        Channel = Convert.ToInt16( rawDetection[ 1 ] ),
                        Name = rawDetection[ 2 ],
                        Location = new GenericLocation<float>( float.Parse( rawDetection[ 3 ], CultureInfo.InvariantCulture.NumberFormat ),
                                                               float.Parse( rawDetection[ 4 ], CultureInfo.InvariantCulture.NumberFormat ) ),
                        FloorPlan = Convert.ToInt16( rawDetection[ 5 ] ),
                        Radius = float.Parse( ( rawDetection[ 6 ] ), CultureInfo.InvariantCulture.NumberFormat ),
                    } );
            }
            DetectorsReceived( this, new SonitorEventArgs( message ) );
        }

        public event SonitorMessageReceivedHandler ProtocolReceived = delegate { };

        void HandleProtocolMessage( List<string> msg )
        {
            var message = new ProtocolVersionMessage( msg[ 1 ] );
            ProtocolReceived( this, new SonitorEventArgs( message ) );
        }

        public event SonitorMessageReceivedHandler DetectionsReceived = delegate { };

        void HandleDetectionMessage( List<string> msg )
        {
            var message = new DetectionsMessage();

            for ( int i = 1; i < msg.Count; i++ )
            {
                var rawDetection = msg[ i ].Split( ',' );
                message.Detections.Add(
                    new Detection
                    {
                        DateTime = new DateTime(
                        Convert.ToInt16( rawDetection[ 0 ] ),
                        Convert.ToInt16( rawDetection[ 1 ] ),
                        Convert.ToInt16( rawDetection[ 2 ] ),
                        Convert.ToInt16( rawDetection[ 3 ] ),
                        Convert.ToInt16( rawDetection[ 4 ] ),
                        Convert.ToInt16( rawDetection[ 5 ] ),
                        Convert.ToInt16( rawDetection[ 6 ] ) ),
                        TagId = rawDetection[ 7 ],
                        HostName = rawDetection[ 8 ],
                        Channel = Convert.ToInt16( rawDetection[ 9 ] ),
                        Amplitude = float.Parse( ( rawDetection[ 10 ] ), CultureInfo.InvariantCulture.NumberFormat ),
                        ConfidenceLevel = float.Parse( ( rawDetection[ 11 ] ), CultureInfo.InvariantCulture.NumberFormat ),
                        MovingStatus = SonitorConverter.ConvertToMovingStatus( Convert.ToInt16( rawDetection[ 12 ] ) ),
                        BatteryStatus = SonitorConverter.ConvertToBatteryStatus( Convert.ToInt16( rawDetection[ 13 ] ) ),
                        ButtonAState = SonitorConverter.ConvertToButtonState( Convert.ToInt16( rawDetection[ 14 ] ) ),
                        ButtonBState = SonitorConverter.ConvertToButtonState( Convert.ToInt16( rawDetection[ 15 ] ) ),
                        ButtonCState = SonitorConverter.ConvertToButtonState( Convert.ToInt16( rawDetection[ 16 ] ) ),
                        ButtonDState = SonitorConverter.ConvertToButtonState( Convert.ToInt16( rawDetection[ 17 ] ) ),
                        SelectedField = SonitorConverter.ConvertToField( Convert.ToInt16( rawDetection[ 18 ] ) )
                    } );
            }

            DetectionsReceived( this, new SonitorEventArgs( message ) );
        }


        public string Name { get; set; }

        public Guid Id { get; set; }

        public void Send( string message )
        {
            throw new Exception( "Location tracker sevices does not support sending data" );
        }
    }
}