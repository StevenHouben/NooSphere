using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NooSphere.Core.ActivityModel;

namespace NooSphere.ActivitySystem.Events
{
    public class FriendEventArgs
    {
        public User User { get; set; }
        public FriendEventArgs() { }
        public FriendEventArgs(User u)
        {
            this.User = u;
        }
    }
    public class FriendDeletedEventArgs
    {
        public Guid Id { get; set; }
        public FriendDeletedEventArgs() { }
        public FriendDeletedEventArgs(Guid id)
        {
            this.Id = id;
        }
    }
}
