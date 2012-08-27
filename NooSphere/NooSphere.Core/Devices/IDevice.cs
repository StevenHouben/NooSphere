namespace NooSphere.Core.Devices
{
    public interface IDevice
    {
         DeviceType DeviceType { get; set; }
         DeviceRole DeviceRole { get; set; }
         DevicePortability DevicePortability { get; set; }

         string Location { get; set; }
         string BaseAddress { get; set; }
    }
}
