using System;
using NooSphere.Model.Primitives;


namespace NooSphere.Model
{
    public class FileResource : Noo
    {
        public string FileType { get; set; }
        public string ActivityId { get; set; }
    }

    public class LegacyResource : Noo
	{
		public LegacyResource()
		{
			InitializeTimeStamps();
		}

		public LegacyResource( int size, string name )
		{
			InitializeTimeStamps();
			Name = name;
			Size = size;
		}

		void InitializeTimeStamps()
		{
			CreationTime = DateTime.Now.ToString( "u" );
			LastWriteTime = DateTime.Now.ToString( "u" );
		}

		Guid activityId;

		public Guid ActivityId
		{
			get { return activityId; }
			set
			{
				activityId = value;
				OnPropertyChanged( "activityId" );
			}
		}

		int size;

		public int Size
		{
			get { return size; }
			set
			{
				size = value;
				OnPropertyChanged( "size" );
			}
		}

		string creationTime;

		public string CreationTime
		{
			get { return creationTime; }
			set
			{
				creationTime = value;
				OnPropertyChanged( "creationTime" );
			}
		}

		string lastWriteTime;

		public string LastWriteTime
		{
			get { return lastWriteTime; }
			set
			{
				lastWriteTime = value;
				OnPropertyChanged( "lastWriteTime" );
			}
		}

		public string RelativePath
		{
			get { return ActivityId + "/" + Name; }
		}

		public string CloudPath
		{
			get { return "Activities/" + ActivityId + "/Resources/" + Id; }
		}
	}
}