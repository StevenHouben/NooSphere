using ABC.Model.Users;


namespace ABC.Infrastructure.ActivityBase
{
    public class UserEventArgs
    {
        public IUser User { get; set; }
        public UserEventArgs() {}

        public UserEventArgs( IUser user )
        {
            User = user;
        }
    }

    public class UserRemovedEventArgs
    {
        public string Id { get; set; }
        public UserRemovedEventArgs() {}

        public UserRemovedEventArgs( string id )
        {
            Id = id;
        }
    }
}