using System.Collections.Generic;
using NooSphere.Model.Primitives;


namespace NooSphere.Model
{
	public class Action : Noo
	{
		public Action()
		{
			InitializeProperties();
		}


		#region Initializers

		void InitializeProperties()
		{
			Resources = new List<LegacyResource>();
		}

		#endregion


		#region Properties

		List<LegacyResource> _resources;

		public List<LegacyResource> Resources
		{
			get { return _resources; }
			set
			{
				_resources = value;
				OnPropertyChanged( "Resources" );
			}
		}

		#endregion
	}
}