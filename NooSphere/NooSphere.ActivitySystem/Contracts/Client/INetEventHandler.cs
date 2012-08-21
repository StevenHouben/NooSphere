using System.ServiceModel;

namespace NooSphere.ActivitySystem.Contracts.Client
{
    [ServiceContract]
    interface INetEventHandler : IServiceBase, IActivityNetEvent, IComNetEvent, IDeviceNetEvent, IFileNetEvent, IUserEvent
    {
    }
}
