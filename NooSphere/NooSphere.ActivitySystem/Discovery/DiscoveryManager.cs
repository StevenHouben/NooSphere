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
using System.ServiceModel.Discovery;
using System.Collections.ObjectModel;

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
        public DiscoveryType DiscoveryType { get; private set; }
        #endregion

        #region Constructor
        public DiscoveryManager()
        { 
            ActivityServices = new List<ServiceInfo>();
        }
        #endregion

        #region Public Members

        /// <summary>
        /// Starts a discovery process
        /// </summary>
        public void Find(DiscoveryType type)
        {
            ActivityServices.Clear();

            if (type == Discovery.DiscoveryType.WS_DISCOVERY)
            {
                using (DiscoveryClient ws_browser = new DiscoveryClient(new UdpDiscoveryEndpoint()))
                {
                    ws_browser.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(ws_browser_FindProgressChanged);
                    ws_browser.FindCompleted += new EventHandler<FindCompletedEventArgs>(ws_browser_FindCompleted);
                    ws_browser.FindAsync(new FindCriteria(typeof(IDiscovery)));
                }
            }
            else if (type == Discovery.DiscoveryType.ZEROCONF)
            {
                ServiceBrowser zc_browser = new ServiceBrowser();
                zc_browser.ServiceAdded += delegate(object o, ServiceBrowseEventArgs args)
                {
                    args.Service.Resolved += new ServiceResolvedEventHandler(zc_browser_Service_Resolved);
                    args.Service.Resolve();
                };
                    zc_browser.Browse("_am._tcp", "local");
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
            ServiceInfo sst = new ServiceInfo(
                Helpers.XML.FromXElement<string>(metaData.Extensions[0]),
                Helpers.XML.FromXElement<string>(metaData.Extensions[1]),
                Helpers.XML.FromXElement<string>(metaData.Extensions[2]));
            ActivityServices.Add(sst);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(sst));
        }
        private void AddFoundServiceFromSCResolvedData(IResolvableService metaData)
        {
            ServiceInfo sst = new ServiceInfo(metaData.Name, "no",metaData.TxtRecord["addr"].ValueString);
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

        void ws_browser_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            OnDiscoveryFinished(new DiscoveryEventArgs());
        }
        #endregion

        #region Event Handlers
        private void ws_browser_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            AddFoundServiceFromWSMetaData(e.EndpointDiscoveryMetadata);
        }
        private void zc_browser_Service_Resolved(object o, ServiceResolvedEventArgs args)
        {
            AddFoundServiceFromSCResolvedData((IResolvableService)args.Service);
        }
        #endregion
    }
}
