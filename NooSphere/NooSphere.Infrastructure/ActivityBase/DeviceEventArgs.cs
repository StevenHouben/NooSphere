using ABC.Model.Device;


namespace ABC.Infrastructure.ActivityBase
{
    public class DeviceEventArgs
    {
        public IDevice Device { get; set; }
        public DeviceEventArgs() {}

        public DeviceEventArgs( IDevice device )
        {
            Device = device;
        }
    }

    public class DeviceRemovedEventArgs
    {
        public string Id { get; set; }
        public DeviceRemovedEventArgs() {}

        public DeviceRemovedEventArgs( string id )
        {
            Id = id;
        }
    }
}