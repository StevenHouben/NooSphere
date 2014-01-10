using ABC.Model.Device;


namespace ABC.Infrastructure.Web
{
    public class ConnectedClient
    {
        public string Name { get; private set; }
        public string Ip { get; private set; }
        public Device Device { get; set; }

        public ConnectedClient( string name, string ip, Device devi )
        {
            Name = name;
            Ip = ip;
            Device = devi;
        }

        public override string ToString()
        {
            return Ip;
        }
    }
}