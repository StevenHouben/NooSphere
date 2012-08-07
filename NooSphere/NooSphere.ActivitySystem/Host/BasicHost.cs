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
using System.ServiceModel.Description;
using System.Threading;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Helpers;

namespace NooSphere.ActivitySystem.Host
{
    public delegate void HostLaunchedHandler(object sender, EventArgs e);

    public delegate void HostClosedHandler(object sender, EventArgs e);

    public class BasicHost
    {
        #region Events

        public event HostLaunchedHandler HostLaunched = null;
        public event HostClosedHandler HostClosed = null;

        #endregion

        #region Members

        private readonly BroadcastService _broadcast = new BroadcastService();
        private ServiceHost _host;
        private ServiceEndpoint _serviceEndpoint;

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

        #endregion

        #region Constructor-Destructor

        /// <summary>
        /// Constructor
        /// </summary>
        public BasicHost()
        {
            Ip = Net.GetIp(IPType.All);
            Port = Net.FindPort();

            Address = "http://" + Ip + ":" + Port + "/";
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~BasicHost()
        {
            Close();
        }

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
        public void StartBroadcast(string hostName, string location = "undefined")
        {
            var t = new Thread(() =>
                                   {
                                       StopBroadcast();
                                       _broadcast.Start(DiscoveryType.Zeroconf, hostName, location,
                                                        Net.GetUrl(Ip, Port, ""));
                                   }) {IsBackground = true};
            t.Start();
        }

        /// <summary>
        /// Stops the broadcast service
        /// </summary>
        public void StopBroadcast()
        {
            if (_broadcast != null)
                if (_broadcast.IsRunning)
                    _broadcast.Stop();
        }

        /// <summary>
        /// Opens a service that is run the host
        /// </summary>
        /// <param name="implementation">The concrete initialized single instance service</param>
        /// <param name="description">The interface or contract of initialized single instance service</param>
        /// <param name="name">The name the service</param>
        public void Open(object implementation, Type description, string name)
        {
            Console.WriteLine("BasicHost: Attemting to find an IP for endPoint");
            Ip = Net.GetIp(IPType.All);

            Console.WriteLine("BasicHost: Found IP " + Ip);
            _host = new ServiceHost(implementation);

            var binding = new WebHttpBinding {MaxReceivedMessageSize = Int32.MaxValue};


            _serviceEndpoint = _host.AddServiceEndpoint(description, binding, Net.GetUrl(Ip, Port, ""));
            _serviceEndpoint.Behaviors.Add(new WebHttpBehavior());

            _host.Faulted += host_Faulted;
            _host.UnknownMessageReceived += host_UnknownMessageReceived;
            _host.Open();

            Console.WriteLine("BasicHost: Host opened at " + Net.GetUrl(Ip, Port, ""));
            IsRunning = true;

            OnHostLaunchedEvent(new EventArgs());
        }

        private void host_UnknownMessageReceived(object sender, UnknownMessageReceivedEventArgs e)
        {
            Console.WriteLine("Unknow message:" + e.Message);
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
                    _host.Close();
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
            Console.WriteLine("BasicHost: Faulted: " + e);
        }

        #endregion
    }
}