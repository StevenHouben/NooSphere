/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System.Collections.Generic;
using NooSphere.Core.Primitives;
using System.ComponentModel;

namespace NooSphere.Core.ActivityModel
{
    public class Action : Noo, INotifyPropertyChanged
    {
        public Action()
        {
            InitializeProperties();
        }

        #region Initializers
        private void InitializeProperties()
        {
            Resources = new List<Resource>();
        }
        #endregion

        #region Properties
        private List<Resource> _resources;
        public List<Resource> Resources
        {
            get { return _resources; }
            set
            {
                _resources = value;
                NotifyPropertyChanged("Resources");
            }
        }
        #endregion
        
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
