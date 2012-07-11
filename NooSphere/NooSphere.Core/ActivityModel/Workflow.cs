using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using NooSphere.Core.ContextModel;
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ActivityModel
{
    public class Workflow : Identity
    {
        public List<Action> Actions{get ;set; }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
