namespace ABC.Infrastructure.ActivityBase
{
    public class NetEventArgs
    {
        public string Raw { get; set; }
        public NetEventArgs() {}

        public NetEventArgs( string raw )
        {
            Raw = raw;
        }
    }
}