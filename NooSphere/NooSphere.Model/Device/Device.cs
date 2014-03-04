using System.Collections.Generic;
using NooSphere.Model.Primitives;
using NooSphere.Model.Users;


namespace NooSphere.Model.Device
{
	public class Device : Noo, IDevice
	{
		public DeviceType DeviceType { get; set; }
		public DeviceRole DeviceRole { get; set; }
		public DevicePortability DevicePortability { get; set; }
		public string TagValue { get; set; }

		public string Location { get; set; }
		public string BaseAddress { get; set; }
		public string ConnectionId { get; set; }

        public IUser Owner { get; set; }

        public List<IUser> Users { get; set; } 

		public Device()
		{
			Type = typeof( IDevice ).Name;
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