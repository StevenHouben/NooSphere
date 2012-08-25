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
using System.Collections.Generic;
using System.Net.Sockets;
using System.ServiceModel.Discovery;
using Mono.Zeroconf;

#if !ANDROID

#else
using System.Net;
using System.Text;
using System.Xml;
#endif

namespace NooSphere.Discovery.Discovery
{
    public class DiscoveryManager
    {
        #region Events
        public event DiscoveryFinishedHander DiscoveryFinished = null;
        public event DiscoveryAddressAddedHandler DiscoveryAddressAdded = null;
        #endregion

        #region Properties
        public delegate void DiscoveryFinishedHander(Object o, DiscoveryEventArgs e);
        public delegate void DiscoveryAddressAddedHandler(Object o, DiscoveryAddressAddedEventArgs e);
        public List<ServiceInfo> ActivityServices { get; set; }
        public DiscoveryType DiscoveryType { get; set; }
        #endregion

        #region Private Members
        private const int WsDiscoveryPort = 3702;
        private const string WsDiscoveryIPAddress = "239.255.255.250";
        private readonly string _messageId;
        private readonly UdpClient _udpClient;
        #endregion

        #region Constructor
        public DiscoveryManager()
        {
            ActivityServices = new List<ServiceInfo>();
            DiscoveryType = DiscoveryType.WSDiscovery;
        #if ANDROID
            _messageId = Guid.NewGuid().ToString();
            _udpClient = new UdpClient(WsDiscoveryPort);
            _udpClient.JoinMulticastGroup(IPAddress.Parse(WsDiscoveryIPAddress));
            _udpClient.BeginReceive(HandleRequest, _udpClient);
        #endif
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts a discovery process
        /// </summary>
        public void Find(DiscoveryType type = DiscoveryType.WSDiscovery)
        {
            ActivityServices.Clear();
#if !ANDROID
            switch (type)
            {
                case DiscoveryType.WSDiscovery:
                    using (var wsBrowser = new DiscoveryClient(new UdpDiscoveryEndpoint()))
                    {
                        wsBrowser.FindProgressChanged += WSBrowserFindProgressChanged;
                        wsBrowser.FindCompleted += WSBrowserFindCompleted;
                        wsBrowser.FindAsync(new FindCriteria(typeof(IDiscovery)));
                    }
                    break;
                case DiscoveryType.Zeroconf:
                    {
                        var zcBrowser = new ServiceBrowser();
                        zcBrowser.ServiceAdded += delegate(object o, ServiceBrowseEventArgs args)
                        {
                            args.Service.Resolved += ZcBrowserServiceResolved;
                            args.Service.Resolve();
                        };
                        zcBrowser.Browse("_am._tcp", "local");
                        
                    }
                    break;
            }

            switch (type)
            {
                case DiscoveryType.WSDiscovery:
                    using (var wsBrowser = new DiscoveryClient(new UdpDiscoveryEndpoint()))
                    {
                        wsBrowser.FindProgressChanged += WSBrowserFindProgressChanged;
                        wsBrowser.FindCompleted += WSBrowserFindCompleted;
                        wsBrowser.FindAsync(new FindCriteria(typeof(IDiscovery)));
                    }
                    break;
                case DiscoveryType.Zeroconf:
                    {
                        var zcBrowser = new ServiceBrowser();
                        zcBrowser.ServiceAdded += delegate(object o, ServiceBrowseEventArgs args)
                        {
                            args.Service.Resolved += ZcBrowserServiceResolved;
                            args.Service.Resolve();
                        };
                        zcBrowser.Browse("_am._tcp", "local");

                    }
                    break;
            }
#else
            Probe();
#endif
        }
        #endregion

        #region Private Methods
#if ANDROID
        private void Probe()
        {
            var data = Encoding.ASCII.GetBytes(ProbeMessage);
            _udpClient.Send(data, data.Length, new IPEndPoint(IPAddress.Parse(WsDiscoveryIPAddress), WsDiscoveryPort));
        }
        private void HandleRequest(IAsyncResult result)
        {
            var endPoint = new IPEndPoint(IPAddress.Any, WsDiscoveryPort);
            var bytes = _udpClient.EndReceive(result, ref endPoint);
            var str = Encoding.ASCII.GetString(bytes);
            var xml = new XmlDocument();
            xml.LoadXml(str);
            var matches = xml.SelectNodes("//*[local-name() = 'ProbeMatch'] | //*[local-name() = 'Hello']");
            if(matches.Count > 0)
            {
                foreach(XmlNode match in matches)
                {
                    var serviceInfo = new ServiceInfo
                                     {
                                         Name = match.SelectNodes("//*[local-name() = 'string']")[0].InnerText,
                                         Location = match.SelectNodes("//*[local-name() = 'string']")[1].InnerText,
                                         Address = match.SelectNodes("//*[local-name() = 'string']")[2].InnerText,
                                         Code = match.SelectNodes("//*[local-name() = 'string']")[3].InnerText
                                     };
                    if (ActivityServices.SingleOrDefault(si => si.Address == serviceInfo.Address) == null)
                    {
                        ActivityServices.Add(serviceInfo);
                        OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(serviceInfo));
                    }
                }
            }
            _udpClient.BeginReceive(HandleRequest, _udpClient);
        }
#else
        /// <summary>
        /// Adds a discovered service to the service list and send a DiscoverAddressAdded event
        /// </summary>
        /// <param name="metaData">The meta data of the service</param>
        private void AddFoundServiceFromWSMetaData(EndpointDiscoveryMetadata metaData)
        {
            var sst = new ServiceInfo(
                Helpers.Xml.FromXElement<string>(metaData.Extensions[0]),
                Helpers.Xml.FromXElement<string>(metaData.Extensions[1]),
                Helpers.Xml.FromXElement<string>(metaData.Extensions[2]),
                Helpers.Xml.FromXElement<string>(metaData.Extensions[3]));
            ActivityServices.Add(sst);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(sst));
        }
        private void AddFoundServiceFromSCResolvedData(IResolvableService metaData)
        {
            var sst = new ServiceInfo(metaData.TxtRecord["name"].ValueString, metaData.TxtRecord["loc"].ValueString,
                                      metaData.TxtRecord["addr"].ValueString, metaData.TxtRecord["code"].ValueString);
            ActivityServices.Add(sst);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(sst));
        }
#endif
        #endregion

        #region Helper
        private string ProbeMessage
        {
            get
            {
                return "<s:Envelope xmlns:s=\"http://www.w3.org/2003/05/soap-envelope\" xmlns:a=\"http://www.w3.org/2005/08/addressing\">" +
                          "<s:Header>" +
                          "<a:Action s:mustUnderstand=\"1\">http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01/Probe</a:Action>" +
                          "<a:MessageID>urn:uuid:"+_messageId+"</a:MessageID>" +
                          "<a:ReplyTo><a:Address>http://www.w3.org/2005/08/addressing/anonymous</a:Address></a:ReplyTo>" +
                          "<a:To s:mustUnderstand=\"1\">urn:docs-oasis-open-org:ws-dd:ns:discovery:2009:01</a:To>" +
                          "</s:Header>" +
                          "<s:Body>" +
                          "<Probe xmlns=\"http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01\"><d:Types xmlns:d=\"http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01\" xmlns:dp0=\"http://tempuri.org/\">dp0:IDiscovery</d:Types>" +
                          "<Duration xmlns=\"http://schemas.microsoft.com/ws/2008/06/discovery\">PT20S</Duration>" +
                          "</Probe>" +
                          "</s:Body>" +
                          "</s:Envelope>";
            }
        }
        #endregion

        #region Internal Event Handlers
        protected void OnDiscoveryFinished(DiscoveryEventArgs e)
        {
            if (DiscoveryFinished != null)
                DiscoveryFinished(this, e);
        }
        protected void OnDiscoveryAddressAdded(DiscoveryAddressAddedEventArgs e)
        {
            if (DiscoveryAddressAdded != null)
                DiscoveryAddressAdded(this, e);
        }

#if !ANDROID
        void WSBrowserFindCompleted(object sender, FindCompletedEventArgs e)
        {
            OnDiscoveryFinished(new DiscoveryEventArgs());
        }
#endif
        #endregion

        #region Event Handlers
#if !ANDROID
        private void WSBrowserFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            AddFoundServiceFromWSMetaData(e.EndpointDiscoveryMetadata);
        }
        private void ZcBrowserServiceResolved(object o, ServiceResolvedEventArgs args)
        {
            AddFoundServiceFromSCResolvedData(args.Service);
        }
#endif
        #endregion
    }
}