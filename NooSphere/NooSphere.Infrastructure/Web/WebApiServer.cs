using System;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin.Hosting;
using Owin;
using Newtonsoft.Json;

using NooSphere.Infrastructure.Web.Controllers;
using NooSphere.Infrastructure.Events;


namespace NooSphere.Infrastructure.Web
{
    public class WebApiServer
    {
        public string Address { get; private set; }
        public int Port { get; private set; }
        static bool Running { get; set; }

        public bool IsRunning
        {
            get { return Running; }
        }

        public void Start( string addr, int port )
        {
            if ( Running )
                return;
            Running = true;
            Address = addr;
            Port = port;
            Task.Factory.StartNew( () =>
            {
                using ( WebApp.Start<WebService>( Helpers.Net.GetUrl( addr, port, "" ).ToString() ) )
                {
                    Console.WriteLine( "WebAPI running on {0}", Helpers.Net.GetUrl( addr, port, "" ) );
                    while ( Running ) {}
                }
            } );
        }

        public void Stop()
        {
            if ( Running )
                Running = false;
        }

    }
    internal class WebService
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration { DependencyResolver = new ControllerResolver() };
            config.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
            config.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.All;
            config.Routes.MapHttpRoute("Default", "{controller}/{id}", new { id = RouteParameter.Optional });


            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(6);

            app.UseWebApi(config);
            app.MapSignalR<EventDispatcher>("", new ConnectionConfiguration { });

            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            }));
        }
    }
}