using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using NooSphere.Infrastructure.ActivityBase;


namespace NooSphere.Infrastructure.Web.Controllers
{
    public class ControllerResolver : IDependencyResolver
    {
        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService( Type serviceType )
        {
            if ( serviceType == typeof( ActivitiesController ) )
                return new ActivitiesController( ActivityService.ActivitySystem );
            if (serviceType == typeof(MessagesController))
                return new MessagesController(ActivityService.Instance);
            if ( serviceType == typeof( DevicesController ) )
                return new DevicesController( ActivityService.ActivitySystem );
            if (serviceType == typeof(ResourcesController))
                return new ResourcesController(ActivityService.ActivitySystem);
            return serviceType == typeof( UsersController ) ? new UsersController( ActivityService.ActivitySystem ) : null;
        }

        public IEnumerable<object> GetServices( Type serviceType )
        {
            return new List<object>();
        }

        public void Dispose()
        {
            // When BeginScope returns 'this', the Dispose method must be a no-op.
        }
    }
}