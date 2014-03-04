using System.Collections.Generic;
using NooSphere.Model.Configuration;
using NooSphere.Model.Users;
using NooSphere.Model.Primitives;
using NooSphere.Model.Resources;


namespace NooSphere.Model
{
    /// <summary>
    /// Activity Base Class
    /// </summary>
    public class Activity : Noo, IActivity
    {
        #region Constructors

        public Activity()
        {
            Type = typeof(IActivity).Name;
            InitializeProperties();
        }

        #endregion


        #region Initializers

        void InitializeProperties()
        {
            Actions = new List<Action>();
            Participants = new List<string>();
            Meta = new Metadata();
            Resources = new List<Resource>();
            FileResources = new List<FileResource>();
        }

        #endregion


        #region Properties

        string owner;

        public string OwnerId
        {
            get { return owner; }
            set
            {
                owner = value;
                OnPropertyChanged("owner");
            }
        }

        List<string> participants;

        public List<string> Participants
        {
            get { return participants; }
            set
            {
                participants = value;
                OnPropertyChanged("participants");
            }
        }

        List<Action> actions;

        public List<Action> Actions
        {
            get { return actions; }
            set
            {
                actions = value;
                OnPropertyChanged("actions");
            }
        }

        ISituatedConfiguration _configuration;

        public ISituatedConfiguration Configuration
        {
            get { return _configuration; }
            set
            {
                _configuration = value;
                OnPropertyChanged("Configurations");
            }
        }

        Metadata meta;

        public Metadata Meta
        {
            get { return meta; }
            set
            {
                meta = value;
                OnPropertyChanged("meta");
            }
        }

        List<Resource> resources;

        public List<Resource> Resources
        {
            get { return resources; }
            set
            {
                resources = value;
                OnPropertyChanged("resouces");
            }
        }

        List<FileResource> fileResources;

        public List<FileResource> FileResources
        {
            get { return fileResources; }
            set
            {
                fileResources = value;
                OnPropertyChanged("fileResources");
            }
        }

        private ActivityState _state;
        public ActivityState State
        {
            get { return this._state; }
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        private FileResource _logo;
        public FileResource Logo
        {
            get { return this._logo; }
            set
            {
                _logo = value;
                OnPropertyChanged("Logo");
            }
        }

        #endregion


        #region Public Methods

        public List<Resource> GetResources()
        {
            return Resources;
        }

        public List<FileResource> GetFileResources()
        {
            return FileResources;
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
    }

    public enum ActivityState
    {
        Open,
        Closed
    }
}