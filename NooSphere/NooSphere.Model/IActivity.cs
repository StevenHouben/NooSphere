﻿using NooSphere.Model.Primitives;
using NooSphere.Model.Resources;
using NooSphere.Model.Users;
using System.Collections.Generic;


namespace NooSphere.Model
{
	public interface IActivity : INoo
	{
		User Owner { get; set; }
		List<string> Participants { get; set; }
		List<Action> Actions { get; set; }
		Metadata Meta { get; set; }
		List<FileResource> FileResources { get; set; }
        List<Resource> Resources { get; set; }
	}
}