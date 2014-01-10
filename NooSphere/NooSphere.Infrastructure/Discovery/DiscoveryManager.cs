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


namespace ABC.Infrastructure.Discovery
{
    public class DiscoveryManager
    {
        #region Events

        public event DiscoveryFinishedHander DiscoveryFinished = null;
        public event DiscoveryAddressAddedHandler DiscoveryAddressAdded = null;

        #endregion


        #region Properties

        public delegate void DiscoveryFinishedHander( Object o, DiscoveryEventArgs e );

        public delegate void DiscoveryAddressAddedHandler( Object o, DiscoveryAddressAddedEventArgs e );

        public List<ServiceInfo> ActivityServices { get; set; }
        public DiscoveryType DiscoveryType { get; set; }

        #endregion


        #region Constructor

        public DiscoveryManager()
        {
            ActivityServices = new List<ServiceInfo>();
            DiscoveryType = DiscoveryType.WsDiscovery;
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
        public void Find( DiscoveryType type = DiscoveryType.WsDiscovery )
        {
            ActivityServices.Clear();
#if !ANDROID
            switch ( type )
            {
                case DiscoveryType.WsDiscovery:
                    using ( var wsBrowser = new DiscoveryClient( new UdpDiscoveryEndpoint() ) )
                    {
                        wsBrowser.FindProgressChanged += WsBrowserFindProgressChanged;
                        wsBrowser.FindCompleted += WsBrowserFindCompleted;
                        wsBrowser.FindAsync( new FindCriteria( typeof( IDiscovery ) ) );
                    }
                    break;
                case DiscoveryType.Zeroconf:
                {
                    var zcBrowser = new ServiceBrowser();
                    zcBrowser.ServiceAdded += delegate( object o, ServiceBrowseEventArgs args )
                    {
                        args.Service.Resolved += ZcBrowserServiceResolved;
                        args.Service.Resolve();
                    };
                    zcBrowser.Browse( "_am._tcp", "local" );
                }
                    break;
            }

            switch ( type )
            {
                case DiscoveryType.WsDiscovery:
                    using ( var wsBrowser = new DiscoveryClient( new UdpDiscoveryEndpoint() ) )
                    {
                        wsBrowser.FindProgressChanged += WsBrowserFindProgressChanged;
                        wsBrowser.FindCompleted += WsBrowserFindCompleted;
                        wsBrowser.FindAsync( new FindCriteria( typeof( IDiscovery ) ) );
                    }
                    break;
                case DiscoveryType.Zeroconf:
                {
                    var zcBrowser = new ServiceBrowser();
                    zcBrowser.ServiceAdded += delegate( object o, ServiceBrowseEventArgs args )
                    {
                        args.Service.Resolved += ZcBrowserServiceResolved;
                        args.Service.Resolve();
                    };
                    zcBrowser.Browse( "_am._tcp", "local" );
                }
                    break;
            }
#else
            Probe();
#endif
        }

        #endregion


        #region Private Methods

        /// <summary>
        /// Adds a discovered service to the service list and send a DiscoverAddressAdded event
        /// </summary>
        /// <param name="metaData">The meta data of the service</param>
        void AddFoundServiceFromWsMetaData( EndpointDiscoveryMetadata metaData )
        {
            var sst = new ServiceInfo(
                Helpers.Xml.FromXElement<string>( metaData.Extensions[ 0 ] ),
                Helpers.Xml.FromXElement<string>( metaData.Extensions[ 1 ] ),
                Helpers.Xml.FromXElement<string>( metaData.Extensions[ 2 ] ),
                Helpers.Xml.FromXElement<string>( metaData.Extensions[ 3 ] ) );
            ActivityServices.Add( sst );
            OnDiscoveryAddressAdded( new DiscoveryAddressAddedEventArgs( sst ) );
        }

        void AddFoundServiceFromScResolvedData( IResolvableService metaData )
        {
            var sst = new ServiceInfo( metaData.TxtRecord[ "name" ].ValueString, metaData.TxtRecord[ "loc" ].ValueString,
                                       metaData.TxtRecord[ "addr" ].ValueString, metaData.TxtRecord[ "code" ].ValueString );
            ActivityServices.Add( sst );
            OnDiscoveryAddressAdded( new DiscoveryAddressAddedEventArgs( sst ) );
        }

        #endregion


        #region Internal Event Handlers

        protected void OnDiscoveryFinished( DiscoveryEventArgs e )
        {
            if ( DiscoveryFinished != null )
                DiscoveryFinished( this, e );
        }

        protected void OnDiscoveryAddressAdded( DiscoveryAddressAddedEventArgs e )
        {
            if ( DiscoveryAddressAdded != null )
                DiscoveryAddressAdded( this, e );
        }

        protected void WsBrowserFindCompleted( object sender, FindCompletedEventArgs e )
        {
            OnDiscoveryFinished( new DiscoveryEventArgs() );
        }

        #endregion


        #region Event Handlers

        void WsBrowserFindProgressChanged( object sender, FindProgressChangedEventArgs e )
        {
            AddFoundServiceFromWsMetaData( e.EndpointDiscoveryMetadata );
        }

        void ZcBrowserServiceResolved( object o, ServiceResolvedEventArgs args )
        {
            AddFoundServiceFromScResolvedData( args.Service );
        }

        #endregion
    }
}