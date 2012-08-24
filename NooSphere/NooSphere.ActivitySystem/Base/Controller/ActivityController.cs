using System.Collections.Generic;
using System.Collections.ObjectModel;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.Core.Devices;

namespace NooSphere.ActivitySystem.Base.Controller
{
    public class ActivityController:ActivityClient
    {
        public ObservableCollection<IActivityProxy> Proxies = new ObservableCollection<IActivityProxy>();

        public ActivityController(Configuration configuration):
            base("",new Device())
        {
            //todo
            switch (configuration)
            {
                    
            }
        }
    }
}
