using ABC.Model.Primitives;


namespace ABC.Model.Device
{
	public class Device : Noo, IDevice
	{
		public DeviceType DeviceType { get; set; }
		public DeviceRole DeviceRole { get; set; }
		public DevicePortability DevicePortability { get; set; }
		public long TagValue { get; set; }

		public string Location { get; set; }
		public string BaseAddress { get; set; }
		public string ConnectionId { get; set; }

		public Device()
		{
			BaseType = typeof( IDevice ).Name;
		}
	}

	public enum DeviceType
	{
		Desktop,
		Laptop,
		SmartPhone,
		Tablet,
		Tabletop,
		WallDisplay,
		Custom,
		Unknown
	}

	public enum DevicePortability
	{
		Stationary,
		Mobile
	}

	public enum DeviceRole
	{
		Master,
		Slave,
		Mediator
	}
}