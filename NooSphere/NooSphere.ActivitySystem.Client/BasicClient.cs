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

using System.ServiceModel;
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Discovery;
using System.ServiceModel.Web;

using NooSphere.ActivitySystem.Contracts;
using NooSphere.ActivitySystem.Contracts.NetEvents;

using NooSphere.Core.ActivityModel;
using NooSphere.Core.Devices;

using NooSphere.ActivitySystem.Client.Events;
using System.ServiceModel.Description;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Web;
using NooSphere.Helpers;
using Newtonsoft.Json.Linq;

namespace NooSphere.ActivitySystem.Client
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class BasicClient : NetEventHandler
    {
        #region Events
        public event ConnectionEstablishedHandler ConnectionEstablished = null;
        #endregion

        #region Private Members
        private Dictionary<Type, ServiceHost> callbackServices = new Dictionary<Type, ServiceHost>();
        #endregion

        #region Properties
        public string IP { get; set; }
        public string ClientName { get; set; }
        public string DeviceID { get; private set; }
        public string ServiceAddress { get; set; }
        public User CurrentParticipant { get; set; }
        #endregion

        #region Constructor
        public BasicClient(string address)
        {
            Connect(address);
        }

        private void Connect(string address)
        {
            this.IP = NetHelper.GetIP(true);
            TestConnection(address);
        }
        #endregion

        #region Internal Event Handlers
        protected void OnConnectionEstablishedEvent(EventArgs e)
        {
            if (ConnectionEstablished != null)
                ConnectionEstablished(this, e);
        }
        private void discoveryClient_FindProgressChanged(object sender, FindProgressChangedEventArgs e)
        {
            Connect(e.EndpointDiscoveryMetadata.Address.ToString());
        }

        private void TestConnection(string addr)
        {
            Console.WriteLine("BasicClient: Found running service at " + addr);
            ServiceAddress = addr;
            bool res = JsonConvert.DeserializeObject<bool>(RestHelper.Get(ServiceAddress));
            Console.WriteLine("BasicClient: Service active? -> " + res);

        }
        #endregion

        #region Private Methods
        private void Discover()
        {
            DiscoveryClient discoveryClient = new DiscoveryClient(new UdpDiscoveryEndpoint());
            discoveryClient.FindAsync(new FindCriteria(typeof(NooSphere.ActivitySystem.Contracts.IActivityManager)));
            discoveryClient.FindProgressChanged += new EventHandler<FindProgressChangedEventArgs>(discoveryClient_FindProgressChanged);
        }
        private Type TypeFromEnum(EventType type)
        {
            switch (type)
            {
                case EventType.ActivityEvents:
                    return typeof(IActivityNetEvent);
                case EventType.ComEvents:
                    return typeof(IComNetEvent);
                case EventType.DeviceEvents:
                    return typeof(IDeviceNetEvent);
                case EventType.FileEvents:
                    return typeof(IFileNetEvent);
                default:
                    return null;
            }
        }
        private int StartCallbackService(Type service)
        {
            int port = NetHelper.FindPort();

            ServiceHost eventHandlerService = new ServiceHost(this);
            ServiceEndpoint se = eventHandlerService.AddServiceEndpoint(service, new WebHttpBinding(), GetUrl(this.IP, port, ""));
            se.Behaviors.Add(new WebHttpBehavior());
            try
            {
                eventHandlerService.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Basic Client: error launching callback service: " + ex.ToString());
                throw new ApplicationException(ex.ToString());
            }
            callbackServices.Add(service, eventHandlerService);
            return port;
        }
        private Uri GetUrl(string ip, int port, string relative)
        {
            return new Uri(string.Format("http://{0}:{1}/{2}", ip, port, relative));
        }
        #endregion

        #region Public Methods
        public void Register()
        {
            Device d = new Device();
            d.BaseAddress = NetHelper.GetIP(true);
            Register(d);
        }
        public void Register(Device d)
        {
            DeviceID = JsonConvert.DeserializeObject<String>(RestHelper.Post(ServiceAddress + Url.devices, null, d));
            Console.WriteLine("BasicClient: Received device id: " + DeviceID);
        }
        public void Unregister(string id)
        {
            RestHelper.Delete(ServiceAddress + Url.devices, null, id);
        }
        public void Subscribe(EventType type)
        {
            int port = StartCallbackService(TypeFromEnum(type));
            var subscription = new
            {
                id = DeviceID,
                port = port,
                type = type
            };
            RestHelper.Post(ServiceAddress + Url.subscribers, null, subscription);
        }
        public void UnSubscribe(EventType type)
        {
            var unSubscription = new
            {
                id = DeviceID,
                type = type
            };
            RestHelper.Delete(ServiceAddress + Url.subscribers, null, unSubscription);
            Type t = TypeFromEnum(type);
            callbackServices[t].Close();
            callbackServices.Remove(t);
        }
        public void UnSubscribeAll()
        {
            foreach (EventType ev in Enum.GetValues(typeof(EventType)))
                UnSubscribe(ev);
        }
        public void AddActivity(Activity act)
        {
            RestHelper.Post(ServiceAddress + Url.activities, CurrentParticipant, act);
        }
        public void RemoveActivity(Guid id)
        {
            RestHelper.Delete(ServiceAddress + Url.activities, CurrentParticipant, id);
        }
        public void UpdateActivity(Activity act)
        {
            RestHelper.Put(ServiceAddress + Url.activities, CurrentParticipant, act);
        }
        public List<Activity> GetActivities()
        {
            string result = RestHelper.Get(ServiceAddress + Url.activities, CurrentParticipant);
            return JsonConvert.DeserializeObject<List<Activity>>(JsonConvert.DeserializeObject(result).ToString());
        }
        public Activity GetActivity(string id)
        {
            return JsonConvert.DeserializeObject<Activity>(RestHelper.Get(ServiceAddress + Url.activities + "/" + id, CurrentParticipant));
        }
        public void SendMessage(string msg)
        {
            var message = new
            {
                id = DeviceID,
                message = msg
            };
            RestHelper.Post(ServiceAddress + Url.messages, null, message);
        }
        #endregion
    }
    public enum Url
    {
        activities,
        devices,
        subscribers,
        messages
    }
}
