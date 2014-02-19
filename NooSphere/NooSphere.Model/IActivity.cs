using NooSphere.Model.Configuration;
using NooSphere.Model.Primitives;
using NooSphere.Model.Users;
using System.Collections.Generic;

namespace NooSphere.Model
{
	public interface IActivity : INoo
	{
		User Owner { get; set; }
		List<User> Participants { get; set; }
		List<Action> Actions { get; set; }
		Metadata Meta { get; set; }
		List<Resource> Resources { get; set; }
	    ISituatedConfiguration Configuration { get; set; }

        Resource Logo { get; set; }
	}
}