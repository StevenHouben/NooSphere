using System;
using System.Diagnostics;


namespace ABC.Infrastructure.Helpers
{
    public class Log
    {
        public static void Out( string sender, string message, LogCode code = LogCode.Log )
        {
            Debug.WriteLine( "[" + DateTime.Now + "]" + sender + "[" + code + "]: " + message );
        }
    }

    public enum LogCode
    {
        Msg,
        Err,
        Ntf,
        Log,
        War,
        Net
    }
}