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
using NooSphere.ActivitySystem.Discovery.Primitives;

namespace NooSphere.ActivitySystem.Discovery.Client
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
        public Collection<EndpointDiscoveryMetadata> RawEndPointMetaData { get; set; }
        #endregion

        #region Constructor
        public DiscoveryManager()
        { 
            ActivityServices = new List<ServiceInfo>();
            RawEndPointMetaData = new Collection<EndpointDiscoveryMetadata>();
        }
        #endregion

        #region Public Members

        /// <summary>
        /// Starts a discovery process
        /// </summary>
        public void Find()
        {
            ActivityServices.Clear();

            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            discoveryClient.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(discoveryClient_FindProgressChanged);
            discoveryClient.FindCompleted += new EventHandler<FindCompletedEventArgs>(discoveryClient_FindCompleted);
            discoveryClient.FindAsync(new FindCriteria(typeof(NooSphere.ActivitySystem.Contracts.IDiscovery)));

        }
        #endregion

        #region Private Members
        /// <summary>
        /// Adds a discovered service to the service list and send a DiscoverAddressAdded event
        /// </summary>
        /// <param name="metaData">The meta data of the service</param>
        private void AddFoundService(EndpointDiscoveryMetadata metaData)
        {
            ServiceInfo sst = new ServiceInfo(
                Helpers.ObjectToXmlHelper.FromXElement<string>(metaData.Extensions[0]),
                Helpers.ObjectToXmlHelper.FromXElement<string>(metaData.Extensions[1]),
                Helpers.ObjectToXmlHelper.FromXElement<string>(metaData.Extensions[2]));
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

        void discoveryClient_FindCompleted(object sender, FindCompletedEventArgs e)
        {
            RawEndPointMetaData.Clear();
            RawEndPointMetaData = e.Result.Endpoints;
        }
        #endregion

        #region Event Handlers
        private void discoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            AddFoundService(e.EndpointDiscoveryMetadata);
        }
        #endregion
    }
}
