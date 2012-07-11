using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Discovery;
using System.Collections.ObjectModel;

namespace NooSphere.ActivitySystem.Discovery
{
    public delegate void DiscoveryFinishedHander(Object o, DiscoveryEventArgs e); 
    public delegate void DiscoveryAddressAddedHandler(Object o,DiscoveryAddressAddedEventArgs e);
    public class DiscoveryManager
    {
        public event DiscoveryFinishedHander DiscoveryFinished = null;
        public event DiscoveryAddressAddedHandler DiscoveryAddressAdded = null;
        public List<ServicePair> ActivityServices = new List<ServicePair>();
        public Collection<EndpointDiscoveryMetadata> RawEndPointMetaData = new Collection<EndpointDiscoveryMetadata>();

        public void Find()
        {
            ActivityServices.Clear();
            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            discoveryClient.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(discoveryClient_FindProgressChanged);
            discoveryClient.FindCompleted += new EventHandler<FindCompletedEventArgs>(discoveryClient_FindCompleted);
            discoveryClient.FindAsync(new FindCriteria(typeof(NooSphere.ActivitySystem.Contracts.IActivityManager)));

        }
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

        void discoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            ServicePair pair = new ServicePair(Helpers.ObjectToXmlHelper.FromXElement<string>(e.EndpointDiscoveryMetadata.Extensions[0]),
                e.EndpointDiscoveryMetadata.Address.ToString());
            ActivityServices.Add(pair);
            OnDiscoveryAddressAdded(new DiscoveryAddressAddedEventArgs(pair));
        }
    }
}
