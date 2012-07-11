using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.ServiceModel.Discovery;
using System.ServiceModel.Web;
using System.ServiceModel.Description;
using System.Xml.Linq;

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
        public void Open(object implementation,Type description,string name)
        {
            Console.WriteLine("BasicHost: Attemting to find an IP for endPoint");
            this.IP = NetHelper.GetIP(true);

            Console.WriteLine("BasicHost: Found IP "+this.IP);
            //host = new ServiceHost( new ActivityManager());
            host = new ServiceHost(implementation);

            ServiceEndpoint se = host.AddServiceEndpoint(description, new WebHttpBinding(), GetUrl(this.IP, this.Port, ""));
            se.Name = "hahaha";
            se.Behaviors.Add(new WebHttpBehavior());
    

            var endpointDiscoveryBehavior = new EndpointDiscoveryBehavior();

            // add the binding information to the endpoint
            endpointDiscoveryBehavior.Extensions.Add(Helpers.ObjectToXmlHelper.ToXElement<string>(name));

            se.Behaviors.Add(endpointDiscoveryBehavior);
            host.Description.Behaviors.Add(new ServiceDiscoveryBehavior());
            host.Description.Endpoints.Add(new UdpDiscoveryEndpoint());
            host.Faulted += new EventHandler(host_Faulted);
            host.Open();

            Console.WriteLine("BasicHost: Host opened at " + GetUrl(this.IP, this.Port, ""));
            IsRunning = true;

            OnHostLaunchedEvent(new EventArgs());
        }
        private Uri GetUrl(string ip, int port, string relative)
        {
            return new Uri(string.Format("http://{0}:{1}/{2}", ip, port, relative));
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
