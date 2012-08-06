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
using System.ServiceModel.Channels;

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

        #region Constructor-Destructor
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="clientName">The name of the client.</param>
        public BasicHost()
        {
            this.IP = NetHelper.GetIP(IPType.All);
            this.Port = NetHelper.FindPort();

            this.Address = "http://" + this.IP + ":" + this.Port + "/";
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicHost() { this.Close(); }
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

        #region Public Methods
        /// <summary>
        /// Starts a broadcast service for the current service
        /// </summary>
        /// <param name="hostName">The name of the service that needs to be broadcasted</param>
        /// <param name="location">The physical location of the service that needs to be broadcasted</param>
        public void StartBroadcast(string hostName, string location="undefined")
        {
            Thread t = new Thread(() =>
            {
                StopBroadcast();
                broadcast.Start(hostName, location, NetHelper.GetUrl(this.IP, this.Port, ""));
            });
            t.IsBackground = true;
            t.Start();
        }

        /// <summary>
        /// Stops the broadcast service
        /// </summary>
        public void StopBroadcast()
        {
            if (broadcast != null)
                if (broadcast.IsRunning)
                    broadcast.Stop();
        }

        /// <summary>
        /// Opens a service that is run the host
        /// </summary>
        /// <param name="implementation">The concrete initialized single instance service</param>
        /// <param name="description">The interface or contract of initialized single instance service</param>
        /// <param name="name">The name the service</param>
        public void Open(object implementation,Type description,string name)
        {
            Console.WriteLine("BasicHost: Attemting to find an IP for endPoint");
            this.IP = NetHelper.GetIP(IPType.All);

            Console.WriteLine("BasicHost: Found IP "+this.IP);
            host = new ServiceHost(implementation);

            WebHttpBinding binding = new WebHttpBinding();
            binding.MaxReceivedMessageSize = 5000000;
 

            serviceEndpoint = host.AddServiceEndpoint(description, binding, NetHelper.GetUrl(this.IP, this.Port, ""));
            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());

            host.Faulted += new EventHandler(host_Faulted);
            host.UnknownMessageReceived += new EventHandler<UnknownMessageReceivedEventArgs>(host_UnknownMessageReceived);
            host.Open();

            Console.WriteLine("BasicHost: Host opened at " + NetHelper.GetUrl(this.IP, this.Port, ""));
            IsRunning = true;

            OnHostLaunchedEvent(new EventArgs());
        }

        private void host_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            Console.WriteLine("Unknow message:" + e.Message.ToString());
        }

        /// <summary>
        /// Closes the service that is running in the host
        /// </summary>
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
