using System.Collections.Generic;


namespace ABC.Infrastructure.Context.Location.Sonitor
{


    #region Converters

    public class SonitorConverter
    {
        public static SonitorMessages DetermineMessage( string raw )
        {
            var dict = new Dictionary<string, SonitorMessages>
            {
                { "DETECTION", SonitorMessages.Detection },
                { "DETECTORS", SonitorMessages.Detectors },
                { "DETECTORSTATUS", SonitorMessages.Detectorstatus },
                { "MAPS", SonitorMessages.Maps },
                { "PROTOCOLVERSION", SonitorMessages.Protocolversion },
                { "TAGS", SonitorMessages.Tags }
            };
            return dict[ raw.ToUpper() ];
        }

        public static bool ConvertToField( int p )
        {
            return p == 1;
        }

        public static ButtonState ConvertToButtonState( int p )
        {
            if ( p == 1 )
                return ButtonState.Pressed;
            return p == 0 ? ButtonState.NotPressed : ButtonState.Undefined;
        }

        public static BatteryStatus ConvertToBatteryStatus( int p )
        {
            if ( p == 0 )
                return BatteryStatus.Ok;
            return p == -1 ? BatteryStatus.Undefined : BatteryStatus.Low;
        }

        public static MovingStatus ConvertToMovingStatus( int p )
        {
            if ( p == -1 )
                return MovingStatus.Undefined;
            return p == 1 ? MovingStatus.Moving : MovingStatus.NonMoving;
        }
    }

    #endregion
}