using System;

namespace NooSphere.ActivitySystem.Context
{
    public delegate void DataReceivedHandler(Object sender, DataEventArgs e);

    public interface IProxy
    {
        string Name { get; set; }

        void Start();
        void Stop();

        event DataReceivedHandler DataReceived;
        event EventHandler Started;
        event EventHandler Stopped;
    }

    public class DataEventArgs
    {
        public object Data { get; set; }

        public DataEventArgs(object data)
        {
            Data = data;
        }
    }

    public enum Source
    {
        Serial,
        Net
    }


}
