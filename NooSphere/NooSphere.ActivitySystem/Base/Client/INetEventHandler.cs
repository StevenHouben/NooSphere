using System.ServiceModel;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Contracts.Client;

namespace NooSphere.ActivitySystem.Base.Client
{
    [ServiceContract]
    interface INetEventHandler : IServiceBase, IActivityNetEvent, IComNetEvent, IDeviceNetEvent, IFileNetEvent, IUserEvent
    {
    }
}
