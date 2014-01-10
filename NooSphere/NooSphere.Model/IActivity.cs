using ABC.Model.Primitives;
using ABC.Model.Users;
using System.Collections.Generic;


namespace ABC.Model
{
	public interface IActivity : INoo
	{
		User Owner { get; set; }
		List<User> Participants { get; set; }
		List<Action> Actions { get; set; }
		Metadata Meta { get; set; }
		List<Resource> Resources { get; set; }
	}
}