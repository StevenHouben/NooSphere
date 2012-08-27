using System;

namespace NooSphere.ActivitySystem.Helpers
{
    public class Log
    {
        public static void Out(string sender,string message,LogCode code=LogCode.Log)
        {
            Console.WriteLine("[" +DateTime.Now + "]" +sender + "["+code+"]: " + message);
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
