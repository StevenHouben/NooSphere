using System;
using System.Windows.Input;


namespace ABC.Model.Primitives
{
	public class Noo : Base, INoo
	{
		public Noo()
		{
			Name = "default";
			Id = Guid.NewGuid().ToString();
			Description = "default";
		}

		public string BaseType { get; set; }

		string _id;

		public string Id
		{
			get { return _id; }
			set
			{
				_id = value;
				OnPropertyChanged( "id" );
			}
		}

		string _name;

		public string Name
		{
			get { return _name; }
			set
			{
				_name = value;
				OnPropertyChanged( "name" );
			}
		}

		string _description;

		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				OnPropertyChanged( "description" );
			}
		}

		string _uri;

		public string Uri
		{
			get { return _uri; }
			set
			{
				_uri = value;
				OnPropertyChanged( "uri" );
			}
		}

		public bool Equals( Noo id )
		{
			return Id == id.Id;
		}


		#region Methods

		public void UpdateAllProperties( object newUser )
		{
			foreach ( var propertyInfo in newUser.GetType().GetProperties() )
				if ( propertyInfo.CanRead )
				{
					var p = propertyInfo.GetValue( newUser, null );
					var o = propertyInfo.GetValue( this, null );
					if ( o != p )
                    {
                        propertyInfo.SetValue(this, propertyInfo.GetValue(newUser, null));
                    }

				}
		}

		#endregion
	}
}