using NooSphere.Model.Primitives;


namespace NooSphere.Model
{
	public class Metadata : Base
	{
		string _header;

		public string Header
		{
			get { return _header; }
			set
			{
				_header = value;
				OnPropertyChanged( "header" );
			}
		}

		string _data;

		public string Data
		{
			get { return _data; }
			set
			{
				_data = value;
				OnPropertyChanged( "data" );
			}
		}
	}
}