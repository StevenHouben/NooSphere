using System;
using System.Diagnostics.Eventing;
using System.Threading;
using NooSphere.Infrastructure.ActivityBase;
using NooSphere.Infrastructure.Discovery;
using NooSphere.Infrastructure.Helpers;
using NooSphere.Model;
using NooSphere.Model.Device;
using NooSphere.Model.Users;

namespace NooSphere.Examples
{
    class ExampleCode
    {
        static void Main(string[] args)
        {

            //Create a user
            var user = new User
            {
                Name = "Steven"
            };

            //Create a device
            var device = new Device
            {
                DeviceType = DeviceType.Desktop,
                DevicePortability = DevicePortability.Stationary
            };

            //create databaseconfiguration
            var databaseConfiguration = new DatabaseConfiguration("127.0.0.1", 8080, "test");

            //create activitysystem: this is the main component that provides an activity-
            //centric wrapper over a pure datastore. This system cam be used by the UI
            //directly through events or be exposed to a REST service using the activityservice
            //and accessed on other devices by using the activityclient
            var activitySystem = new ActivitySystem(databaseConfiguration) { Device = device };

            //add handlers to the system for local UI support.
            activitySystem.ActivityAdded+=activitySystem_ActivityAdded;
            activitySystem.DeviceAdded += activitySystem_DeviceAdded;
            activitySystem.UserAdded += activitySystem_UserAdded;

            //Start a activityservice which wraps an activity system into a REST service
            var activityService = new ActivityService(activitySystem, "127.0.0.1", 8060);
            activityService.Start();

            //make the system discoverable on the LAN
            activityService.StartBroadcast(DiscoveryType.Zeroconf, "activitySystem", "mycompany");

            //Wait for one second to allow the service to start
            Thread.Sleep(1000);

            //Use the discoverymanager to find the activityservice and connect to it
            //using an activityclient. This should be used to connect to the
            //activity system from a different device.
            var disco = new DiscoveryManager();

            disco.DiscoveryAddressAdded += (sender, e) =>
            {
                var foundWebConfiguration = new WebConfiguration(e.ServiceInfo.Address);

                //Note that you don't have to use discovery but can connect directly
                //if you know the IP
                var activityClient = new ActivityClient(foundWebConfiguration.Address, foundWebConfiguration.Port, device);
                activityClient.ActivityAdded += activityClient_ActivityAdded;
                activityClient.UserAdded += activityClient_UserAdded;
                activityClient.DeviceAdded += activityClient_DeviceAdded;
            };

            disco.Find(DiscoveryType.Zeroconf);


            //To produce test data
            while (true)
            {
                Console.WriteLine("Press a + enter for new activity, press u + enter for new user");
                var input = Console.ReadLine();
                if(input == "a")
                    activitySystem.AddActivity(new Activity());
                if(input == "u")
                    activitySystem.AddUser(new User());
            }

        }

        static void activityClient_DeviceAdded(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("Device {0} received from activityclient over http", e.Device.Name);
        }

        static void activityClient_UserAdded(object sender, UserEventArgs e)
        {
            Console.WriteLine("User {0} received from activityclient over http", e.User.Name);
        }

        static void activityClient_ActivityAdded(object sender, Infrastructure.ActivityEventArgs e)
        {
            Console.WriteLine("Activity {0} received from activityclient over http", e.Activity.Name);
        }

        static void activitySystem_UserAdded(object sender, UserEventArgs e)
        {
            Console.WriteLine("User {0} received directly from activitysystem", e.User.Name);
        }

        static void activitySystem_DeviceAdded(object sender, DeviceEventArgs e)
        {
            Console.WriteLine("Device {0} received directly from activitysystem", e.Device.Name);
        }

        static void activitySystem_ActivityAdded(object sender, Infrastructure.ActivityEventArgs e)
        {
            Console.WriteLine("Activity {0} received directly from activitysystem", e.Activity.Name);
        }
    }
}
