namespace ABC.Model.Primitives
{
	public class Rgb : Base
	{
		byte _red;

		public byte Red
		{
			get { return _red; }
			set
			{
				_red = value;
				OnPropertyChanged( "red" );
			}
		}

		byte _blue;

		public byte Blue
		{
			get { return _blue; }
			set
			{
				_blue = value;
				OnPropertyChanged( "blue" );
			}
		}

		byte _green;

		public byte Green
		{
			get { return _green; }
			set
			{
				_green = value;
				OnPropertyChanged( "green" );
			}
		}

		public Rgb( byte red, byte green, byte blue )
		{
			Red = red;
			Green = green;
			Blue = blue;
		}

		public override string ToString()
		{
			return "C" + Red + " " + Green + " " + Blue + "#";
		}
	}

	public class Rgbs
	{
		public static Rgb Red
		{
			get { return new Rgb( 255, 0, 0 ); }
		}

		public static Rgb Green
		{
			get { return new Rgb( 0, 255, 0 ); }
		}

		public static Rgb Blue
		{
			get { return new Rgb( 0, 0, 255 ); }
		}

		public static Rgb Yellow
		{
			get { return new Rgb( 255, 255, 0 ); }
		}

		public static Rgb Cyan
		{
			get { return new Rgb( 0, 255, 255 ); }
		}

		public static Rgb Magenta
		{
			get { return new Rgb( 255, 0, 255 ); }
		}

		public static Rgb Black
		{
			get { return new Rgb( 0, 0, 0 ); }
		}

		public static Rgb White
		{
			get { return new Rgb( 255, 255, 255 ); }
		}

		public static Rgb Gray
		{
			get { return new Rgb( 192, 192, 192 ); }
		}
	}
}