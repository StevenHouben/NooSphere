﻿using System.Collections.Generic;
using NooSphere.Model.Configuration;
using NooSphere.Model.Users;
using NooSphere.Model.Primitives;


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
			BaseType = typeof( IActivity ).Name;
			InitializeProperties();
		}

		#endregion


		#region Initializers

		void InitializeProperties()
		{
			Actions = new List<Action>();
			Participants = new List<User>();
			Meta = new Metadata();
			Resources = new List<Resource>();
		}

		#endregion


		#region Properties

		User _owner;

		public User Owner
		{
			get { return _owner; }
			set
			{
				_owner = value;
				OnPropertyChanged( "owner" );
			}
		}

		List<User> _participants;

		public List<User> Participants
		{
			get { return _participants; }
			set
			{
				_participants = value;
				OnPropertyChanged( "participants" );
			}
		}

		List<Action> _actions;

		public List<Action> Actions
		{
			get { return _actions; }
			set
			{
				_actions = value;
				OnPropertyChanged( "actions" );
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

		Metadata _meta;

		public Metadata Meta
		{
			get { return _meta; }
			set
			{
				_meta = value;
				OnPropertyChanged( "meta" );
			}
		}

		List<Resource> resources;

		public List<Resource> Resources
		{
			get { return resources; }
			set
			{
				resources = value;
				OnPropertyChanged( "resouces" );
			}
		}

        private ActivityState _state;
        public ActivityState State
        {
            get { return this._state;}
            set
            {
                _state = value;
                OnPropertyChanged("State");
            }
        }

        private Resource _logo;
        public Resource Logo
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

		#endregion


		#region Overrides

		public override string ToString()
		{
			return Name;
		}

		public bool Equals( Activity act )
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