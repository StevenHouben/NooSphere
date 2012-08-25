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
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.ActivitySystem.Helpers;

namespace NooSphere.ActivitySystem.Host
{
    public delegate void HostLaunchedHandler(object sender, EventArgs e);

    public delegate void HostClosedHandler(object sender, EventArgs e);

    public class GenericHost
    {
        #region Events

        public event HostLaunchedHandler HostLaunched = null;
        public event HostClosedHandler HostClosed = null;

        #endregion

        #region Members
        private readonly BroadcastService _broadcast = new BroadcastService();
        private ServiceHost _host;
        #endregion

        #region Properties

        /// <summary>
        /// Local callback address
        /// </summary>
        /// <remarks>
        /// Composed of IP and Port
        /// </remarks>
        public string Address { get; private set; }

        /// <summary>
        /// Client IP
        /// </summary>
        public string Ip { get; private set; }

        /// <summary>
        /// Local callback port
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Indicates if the service is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// The service that is running
        /// </summary>
        public IServiceBase Service { get; set; }

        #endregion

        #region Constructor-Destructor

        /// <summary>
        /// Constructor
        /// </summary>
        public GenericHost(int port = -1)
        {
            if (port == -1)
                port = Net.FindPort();
            Ip = Net.GetIp(IPType.All);
            Port = port;

            Address = "http://" + Ip + ":" + Port + "/";
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~GenericHost()
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
        /// <param name="type">Type of discovery </param>
        /// <param name="hostName">The name of the service that needs to be broadcasted</param>
        /// <param name="location">The physical location of the service that needs to be broadcasted</param>
        public void StartBroadcast(DiscoveryType type,string hostName,string code, string location = "undefined")
        {
            var t = new Thread(() =>
                                   {
                                       StopBroadcast();
                                       _broadcast.Start(type, hostName, location,code,
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
        public void Open(IServiceBase implementation, Type description, string name)
        {
            Service = implementation;

            Log.Out("BasicHost", string.Format(" Attemting to find an IP for endPoint"), LogCode.Net);
            Ip = Net.GetIp(IPType.All);

            _host = new ServiceHost(implementation);

            var serviceEndpoint = _host.AddServiceEndpoint(
                description, 
                new WebHttpBinding 
                {
                    MaxReceivedMessageSize = 2147483647,
                    MaxBufferSize = 2147483647,
                    ReaderQuotas = { MaxArrayLength = 2147483647, MaxStringContentLength = 2147483647 }
                }, 
                Net.GetUrl(Ip, Port, ""));

            serviceEndpoint.Behaviors.Add(new WebHttpBehavior());
            _host.Open();

            Log.Out("BasicHost", string.Format(implementation+" host opened at " + Net.GetUrl(Ip, Port, "")), LogCode.Net);
            IsRunning = true;

            OnHostLaunchedEvent(new EventArgs());
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
                    Service.ServiceDown();
                    OnHostClosedEvent(new EventArgs());
                    _host.Close();
                }
                catch (Exception ex)
                {
                    Log.Out("BasicHost", string.Format("Problem closing connection: " + ex.StackTrace), LogCode.Net);
                }
            }
        }

        #endregion
    }
}