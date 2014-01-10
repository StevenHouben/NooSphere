using System.ComponentModel;


namespace ABC.Model.Primitives
{
	public class Base : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged( string name )
		{
			var handler = PropertyChanged;
			if ( handler != null )
			{
				handler( this, new PropertyChangedEventArgs( name ) );
			}
		}
	}
}