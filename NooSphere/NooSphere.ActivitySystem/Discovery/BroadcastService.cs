/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.ServiceModel.Description;
using NooSphere.ActivitySystem.Helpers;
using Mono.Zeroconf;

namespace NooSphere.ActivitySystem.Discovery
{
    public class BroadcastService
    {
        #region Private Members
        private ServiceHost _discoveryHost;
        #endregion

        #region Properties
        /// <summary>
        /// Local callback address
        /// </summary>
        /// <remarks>
        /// Composed of IP and Port
        /// </remarks>
        public string Address { get; set; }

        /// <summary>
        /// Client IP
        /// </summary>
        private string Ip { get; set; }

        /// <summary>
        /// Local callback port
        /// </summary>
        private int Port { get; set; }

        /// <summary>
        /// Indicates if the service is running
        /// </summary>
        public bool IsRunning { get; set; }

        /// <summary>
        /// Type of discovery
        /// </summary>
        public DiscoveryType DiscoveryType { get; set; }
        #endregion

        #region Constructor
        public BroadcastService()
        {
            IsRunning = false;
        }
        #endregion

        #region Public Members

        /// <summary>
        /// Start new broadcast service
        /// </summary>
        /// <param name="type">Type of discovery</param>
        /// <param name="nameToBroadcast">The name of the service that needs to be broadcasted</param>
        /// <param name="physicalLocation">The physical location of the service that needs to be broadcasted</param>
        /// <param name="addressToBroadcast">The address of the service that needs to be broadcasted</param>
        /// <param name="broadcastPort">The port of the broadcast service. Default=56789</param>
        public void Start(DiscoveryType type,string nameToBroadcast,string physicalLocation,string code,Uri addressToBroadcast,int broadcastPort=7892)
        {
            DiscoveryType = type;

            switch (DiscoveryType)
            {
                case DiscoveryType.WSDiscovery:
                    {
                        Ip = Net.GetIp(IPType.All);
                        Port = broadcastPort;
                        Address = "http://" + Ip + ":" + Port + "/";

                        _discoveryHost = new ServiceHost(new DiscoveyService());

                        var serviceEndpoint = _discoveryHost.AddServiceEndpoint(typeof(IDiscovery), new WebHttpBinding(), 
                                                                                Net.GetUrl(Ip, Port, ""));
                        serviceEndpoint.Behaviors.Add(new WebHttpBehavior());

                        var broadcaster = new EndpointDiscoveryBehavior();

                        broadcaster.Extensions.Add(nameToBroadcast.ToXElement<string>());
                        broadcaster.Extensions.Add(physicalLocation.ToXElement<string>());
                        broadcaster.Extensions.Add(addressToBroadcast.ToString().ToXElement<string>());
                        broadcaster.Extensions.Add(code.ToXElement<string>());

                        serviceEndpoint.Behaviors.Add(broadcaster);
                        _discoveryHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
                        _discoveryHost.Description.Endpoints.Add(new UdpDiscoveryEndpoint());
                        _discoveryHost.Open();

                        IsRunning = true;
                    }
                    break;
                case DiscoveryType.Zeroconf:
                    {
                        var service = new RegisterService
                                          {Name = nameToBroadcast, RegType = "_am._tcp", ReplyDomain = "local.", Port = 3689};

                        // TxtRecords are optional
                        var txtRecord = new TxtRecord(){
                                                {"name", nameToBroadcast},
                                                {"addr", addressToBroadcast.ToString()},
                                                {"loc", physicalLocation},
                                                {"code", code}
                                            };
                        service.TxtRecord = txtRecord;

                        service.Register();
                    }
                    break;
            }
        }

        /// <summary>
        /// Stops the broadcast service
        /// </summary>
        public void Stop()
        {
            if (DiscoveryType != DiscoveryType.WSDiscovery) return;
            _discoveryHost.Close();
            IsRunning = false;
        }
        #endregion
    }
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single, IncludeExceptionDetailInFaults = true)]
    public class DiscoveyService : IDiscovery
    {
        public bool Alive()
        { return true; }
        public void  ServiceDown()
        {
        }
    }
}
