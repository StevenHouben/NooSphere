using System.Collections.Generic;
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
			Type = typeof( IActivity ).Name;
			InitializeProperties();
		}

		#endregion


		#region Initializers

		void InitializeProperties()
		{
			Actions = new List<Action>();
			Participants = new List<User>();
			Meta = new Metadata();
			FileResources = new List<FileResource>();
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

		List<FileResource> fileResources;

		public List<FileResource> FileResources
		{
			get { return fileResources; }
			set
			{
				fileResources = value;
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

		#endregion


		#region Public Methods

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