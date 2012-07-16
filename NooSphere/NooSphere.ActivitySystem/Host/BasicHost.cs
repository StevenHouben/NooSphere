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
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.Xml.Linq;
using NooSphere.Helpers;
using NooSphere.ActivitySystem.Discovery.Host;
using System.Threading;

namespace NooSphere.ActivitySystem.Host
{
    public delegate void HostLaunchedHandler(object sender,EventArgs e);
    public delegate void HostClosedHandler(object sender,EventArgs e);
    public class BasicHost
    {
        #region Events
        public event HostLaunchedHandler HostLaunched = null;
        public event HostClosedHandler HostClosed = null; 
        #endregion

        #region Members
        private ServiceHost host;
        private BroadcastService broadcast = new BroadcastService();
        private ServiceEndpoint serviceEndpoint;
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

        #region Local Handler
        protected void OnHostLaunchedEvent(EventArgs e)
        {
            if (HostLaunched != null)
                HostLaunched(this, e);
        }
        protected void OnHostClosedEvent(EventArgs e)
        {
            if (HostClosed != null)
                HostClosed(this, e);
        }
        #endregion

        #region Constructor-Destructor
        /// <summary>
        /// Constructure
        /// </summary>
        /// <param name="clientName">The name of the client.</param>
        public BasicHost()
        {
            this.IP = NetHelper.GetIP(true);
            this.Port = NetHelper.FindPort();
			
			this.Address = "http://"+this.IP+":"+this.Port+"/";
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicHost() { this.Close(); }
        #endregion

        #region Public Methods
        public void StartBroadcast(string hostName, string location)
        {
            Thread t = new Thread(() =>
            {
                StopBroadcast();
                broadcast.Start(hostName, location, NetHelper.GetUrl(this.IP, this.Port, ""));
            });
            t.Start();
        }
        public void StopBroadcast()
        {
            if (broadcast != null)
                if (broadcast.IsRunning)
                    broadcast.Stop();
        }
        public void Open(object implementation,Type description,string name)
        {
            Console.WriteLine("BasicHost: Attemting to find an IP for endPoint");
            this.IP = NetHelper.GetIP(true);

            Console.WriteLine("BasicHost: Found IP "+this.IP);
            host = new ServiceHost(implementation);

            serviceEndpoint = host.AddServiceEndpoint(description, new WebHttpBinding(), NetHelper.GetUrl(this.IP, this.Port, ""));
            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());


            //broadcaster = new EndpointDiscoveryBehavior();

            //// add the binding information to the endpoint
            //broadcaster.Extensions.Add(Helpers.ObjectToXmlHelper.ToXElement<string>(name));

            //serviceEndpoint.Behaviors.Add(broadcaster);
            //host.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            //host.Description.Endpoints.Add(new UdpDiscoveryEndpoint());
            host.Faulted += new EventHandler(host_Faulted);
            host.Open();

            Console.WriteLine("BasicHost: Host opened at " + NetHelper.GetUrl(this.IP, this.Port, ""));
            IsRunning = true;

            OnHostLaunchedEvent(new EventArgs());
        }

        public void Close()
        {
            if (IsRunning)
            {
                try
                {
                    OnHostClosedEvent(new EventArgs());
                    host.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("BasicHost: Problem closing connection: " + ex.StackTrace);
                }
            }
                
        }
        #endregion

        #region Event Handlers
        private void host_Faulted(object sender, EventArgs e)
        {
            Console.WriteLine("BasicHost: Faulted: " + e.ToString());
        }
        #endregion

    }
}
