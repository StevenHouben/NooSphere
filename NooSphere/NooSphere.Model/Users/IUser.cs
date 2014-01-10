using ABC.Model.Primitives;
using System.Collections.Generic;


namespace ABC.Model.Users
{
	public interface IUser : INoo
	{
		string Cid { get; set; }

		string Tag { get; set; }

		string Image { get; set; }

		string Email { get; set; }

		Rgb Color { get; set; }

		bool Selected { get; set; }

		int State { get; set; }

		List<Activity> Activities { get; set; }
	}
}