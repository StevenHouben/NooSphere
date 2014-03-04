using NooSphere.Model.Primitives;
using NooSphere.Model.Resources;
﻿using NooSphere.Model.Configuration;
using NooSphere.Model.Users;
using System.Collections.Generic;

namespace NooSphere.Model
{
	public interface IActivity : INoo
	{
		string OwnerId { get; set; }
		List<string> Participants { get; set; }
		List<Action> Actions { get; set; }
		Metadata Meta { get; set; }
		List<FileResource> FileResources { get; set; }
        List<Resource> Resources { get; set; }

	    ISituatedConfiguration Configuration { get; set; }

        FileResource Logo { get; set; }
	}
}