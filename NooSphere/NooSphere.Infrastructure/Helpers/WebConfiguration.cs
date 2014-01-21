using System;

namespace ABC.Infrastructure.Helpers
{
    public class WebConfiguration
    {
        public int Port { get; set; }
        public string Address { get; set; }

        public Uri Uri { get; set; }

        public WebConfiguration(string address, int port)
        {
            Address = address;
            Port = port;
            Uri = Net.GetUrl(address, port, "");
        }
        public WebConfiguration(string url)
        {
            Uri = new UriBuilder(url).Uri;
            Address = Uri.Host;
            Port = Uri.Port;
        }

        public static WebConfiguration DefaultWebConfiguration = new WebConfiguration(Net.GetIp(IpType.All), 8080);
        public static WebConfiguration LocalWebConfiguration = new WebConfiguration("127.0.0.1",8080);
    }
}
