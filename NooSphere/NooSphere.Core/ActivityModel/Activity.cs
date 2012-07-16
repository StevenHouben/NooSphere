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
using NooSphere.Core;
using System.ComponentModel;
using NooSphere.Core.ContextModel;
using NooSphere.Core.Primitives;
using NooSphere.Core.ContextModel.ComponentModel;


namespace NooSphere.Core.ActivityModel
{
    /// <summary>
    /// Activity Base Class
    /// </summary>
    public class Activity : Identity, INotifyPropertyChanged
    {
        #region Constructors
        public Activity() : base()
        {
            InitializeProperties();
        }
        #endregion

        #region Initializers
        private void InitializeProperties()
        {
            this.Actions = new List<Action>();
            this.Workflows = new List<Workflow>();
            this.Participants = new List<User>();
            this.Meta = new Metadata();
        }
        #endregion

        #region Properties
        public User Owner { get; set; }
        public List<User> Participants { get; set; }

        /// <summary>
        /// Context is a generic object so it can be included
        /// into serialisation.
        /// </summary>
        private object _context;
        public object Context
        {
            get { return _context; }
            set
            {
                this._context = value;
                NotifyPropertyChanged("Context");

            }
        }
        private List<Action> _actions;
        public List<Action> Actions
        {
            get { return _actions; }
            set
            {
                this._actions = value;
                NotifyPropertyChanged("Actions");

            }
        }
        private List<Workflow> _workflows;
        public List<Workflow> Workflows
        {
            get { return _workflows; }
            set
            {
                this._workflows = value;
                NotifyPropertyChanged("Workflows");

            }
        }
        private Metadata _meta;
        public Metadata Meta
        {
            get { return _meta; }
            set
            {
                this._meta = value;
                NotifyPropertyChanged("Metae");

            }
        }
        #endregion

        public List<Resource> GetResources()
        {
            List<Resource> resources = new List<Resource>();

            foreach (Action a in Actions)
                resources.AddRange(a.Resources);

            return resources;
        }

        #region Overrides
        public override string ToString()
        {
            return Name;
        }
        public bool Equals(Activity act)
        {
            return this.Id == act.Id;
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

        #region Lifecycle
        public bool Suspendable { get; set; }
        public bool Resumable { get; set; }
        public bool Shareable { get; set; }
        public bool Roameable { get; set; }
        public bool Externalizeable{get;set;}
        #endregion
    }
}
