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
using System.ServiceModel.Discovery;

using Mono.Zeroconf;

namespace NooSphere.ActivitySystem.Discovery
{
    public delegate void DiscoveryFinishedHander(Object o, DiscoveryEventArgs e); 
    public delegate void DiscoveryAddressAddedHandler(Object o,DiscoveryAddressAddedEventArgs e);
    public class DiscoveryManager
    {
        #region Events
        public event DiscoveryFinishedHander DiscoveryFinished = null;
        public event DiscoveryAddressAddedHandler DiscoveryAddressAdded = null;
        #endregion

        #region Properties
        public List<ServiceInfo> ActivityServices { get; set; }
        public DiscoveryType DiscoveryType { get; set; }
        #endregion

        #region Constructor
        public DiscoveryManager()
        {
            ActivityServices = new List<ServiceInfo>();
            DiscoveryType = DiscoveryType.WSDiscovery;
        }

        #endregion

        #region Public Members

        /// <summary>
        /// Starts a discovery process
        /// </summary>
        public void Find(DiscoveryType type)
        {
            ActivityServices.Clear();

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
        }

        #endregion

        #region Private Members
        /// <summary>
        /// Adds a discovered service to the service list and send a DiscoverAddressAdded event
        /// </summary>
        /// <param name="metaData">The meta data of the service</param>
        private void AddFoundServiceFromWSMetaData(EndpointDiscoveryMetadata metaData)
        {
            var sst = new ServiceInfo(
                Helpers.Xml.FromXElement<string>(metaData.Extensions[0]),
                Helpers.Xml.FromXElement<string>(metaData.Extensions[1]),
                Helpers.Xml.FromXElement<string>(metaData.Extensions[2]));
            ActivityServices.Add(sst);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(sst));
        }
        private void AddFoundServiceFromSCResolvedData(IResolvableService metaData)
        {
            var sst = new ServiceInfo(metaData.Name, "no",metaData.TxtRecord["addr"].ValueString);
            ActivityServices.Add(sst);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(sst));
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

        void WSBrowserFindCompleted(object sender, FindCompletedEventArgs e)
        {
            OnDiscoveryFinished(new DiscoveryEventArgs());
        }
        #endregion

        #region Event Handlers
        private void WSBrowserFindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            AddFoundServiceFromWSMetaData(e.EndpointDiscoveryMetadata);
        }
        private void ZcBrowserServiceResolved(object o, ServiceResolvedEventArgs args)
        {
            AddFoundServiceFromSCResolvedData(args.Service);
        }
        #endregion
    }
}
