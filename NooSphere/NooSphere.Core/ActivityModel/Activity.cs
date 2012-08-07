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
using System.ComponentModel;
using NooSphere.Core.Primitives;

namespace NooSphere.Core.ActivityModel
{
    /// <summary>
    /// Activity Base Class
    /// </summary>
    public class Activity : Base, INotifyPropertyChanged
    {
        #region Constructors

        public Activity()
        {
            InitializeProperties();
        }

        #endregion

        #region Initializers

        private void InitializeProperties()
        {
            Actions = new List<Action>();
            Workflows = new List<Workflow>();
            Participants = new List<User>();
            Meta = new Metadata();
        }

        #endregion

        #region Properties

        private List<Action> _actions;

        /// <summary>
        /// Context is a generic object so it can be included
        /// into serialisation.
        /// </summary>
        private object _context;

        private Metadata _meta;
        private List<Workflow> _workflows;

        public User Owner { get; set; }
        public List<User> Participants { get; set; }

        public object Context
        {
            get { return _context; }
            set
            {
                _context = value;
                NotifyPropertyChanged("Context");
            }
        }

        public List<Action> Actions
        {
            get { return _actions; }
            set
            {
                _actions = value;
                NotifyPropertyChanged("Actions");
            }
        }

        public List<Workflow> Workflows
        {
            get { return _workflows; }
            set
            {
                _workflows = value;
                NotifyPropertyChanged("Workflows");
            }
        }

        public Metadata Meta
        {
            get { return _meta; }
            set
            {
                _meta = value;
                NotifyPropertyChanged("Metae");
            }
        }

        #endregion

        #region Public Methods

        public List<Resource> GetResources()
        {
            var resources = new List<Resource>();

            foreach (Action a in Actions)
                resources.AddRange(a.Resources);

            return resources;
        }

        #endregion

        #region Overrides

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Activity act)
        {
            return Id == act.Id;
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Lifecycle

        public bool Suspendable { get; set; }
        public bool Resumable { get; set; }
        public bool Shareable { get; set; }
        public bool Roameable { get; set; }
        public bool Externalizeable { get; set; }

        #endregion
    }
}