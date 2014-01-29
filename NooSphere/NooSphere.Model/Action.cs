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
			FileResources = new List<LegacyResource>();
		}

		#endregion


		#region Properties

		List<LegacyResource> _fileResources;

		public List<LegacyResource> FileResources
		{
			get { return _fileResources; }
			set
			{
				_fileResources = value;
				OnPropertyChanged( "Resources" );
			}
		}

		#endregion
	}
}