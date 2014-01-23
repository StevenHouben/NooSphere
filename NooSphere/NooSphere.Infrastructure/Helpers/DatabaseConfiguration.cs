using System;

namespace NooSphere.Infrastructure.Helpers
{
    public class DatabaseConfiguration
    {
        public int Port { get; set; }
        public string Address { get; set; }

        public string DatabaseName { get; set; }

        public Uri Uri { get; set; }

        public DatabaseConfiguration(string address, int port,string databaseName)
        {
            Address = address;
            Port = port;
            DatabaseName = databaseName;
            Uri = Net.GetUrl(address, port, "");
        }
        public DatabaseConfiguration(string url)
        {
            Uri = new UriBuilder(url).Uri;
            Address = Uri.Host;
            Port = Uri.Port;
        }

        public static DatabaseConfiguration DefaultDatabaseConfiguration = new DatabaseConfiguration(
            WebConfiguration.DefaultWebConfiguration.Address,
            WebConfiguration.DefaultWebConfiguration.Port,"");
    }
}
