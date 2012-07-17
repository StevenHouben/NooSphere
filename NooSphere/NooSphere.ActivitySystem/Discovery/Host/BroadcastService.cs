/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.ServiceModel.Description;
using NooSphere.ActivitySystem.Contracts;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.Discovery.Host
{
    public class BroadcastService
    {
        #region Private Members
        private ServiceHost discoveryHost;
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
        private string IP { get; set; }

        /// <summary>
        /// Local callback port
        /// </summary>
        private int Port { get; set; }

        /// <summary>
        /// Indicates if the service is running
        /// </summary>
        public bool IsRunning { get; set; }
        #endregion

        #region Constructor
        public BroadcastService()
        {
            this.IsRunning = false;
        }
        #endregion

        #region Public Members
        /// <summary>
        /// Start new broadcast service
        /// </summary>
        /// <param name="nameToBroadcast">The name of the service that needs to be broadcasted</param>
        /// <param name="physicalLocation">The physical location of the service that needs to be broadcasted</param>
        /// <param name="addressToBroadcast">The address of the service that needs to be broadcasted</param>
        /// <param name="broadcastPort">The port of the broadcast service. Default=56789</param>
        public void Start(string nameToBroadcast,string physicalLocation,Uri addressToBroadcast,int broadcastPort=56789)
        {
            this.IP = NetHelper.GetIP(true);
            this.Port = broadcastPort;
            this.Address = "http://" + this.IP + ":" + this.Port + "/";

            discoveryHost = new ServiceHost(new DiscoveyService());

            ServiceEndpoint serviceEndpoint = discoveryHost.AddServiceEndpoint(typeof(IDiscovery), new WebHttpBinding(), NetHelper.GetUrl(this.IP,this.Port,""));
            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());

            EndpointDiscoveryBehavior broadcaster = new EndpointDiscoveryBehavior();

            broadcaster.Extensions.Add(Helpers.ObjectToXmlHelper.ToXElement<string>(nameToBroadcast));
            broadcaster.Extensions.Add(Helpers.ObjectToXmlHelper.ToXElement<string>(physicalLocation));
            broadcaster.Extensions.Add(Helpers.ObjectToXmlHelper.ToXElement<string>(addressToBroadcast.ToString()));

            serviceEndpoint.Behaviors.Add(broadcaster);
            discoveryHost.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            discoveryHost.Description.Endpoints.Add(new UdpDiscoveryEndpoint());
            discoveryHost.Open();

            IsRunning = true;
        }
        
        /// <summary>
        /// Stops the broadcast service
        /// </summary>
        public void Stop()
        {
            discoveryHost.Close();
            IsRunning = false;
        }
        #endregion
    }
}
