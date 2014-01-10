using System;
using System.Collections.Generic;
using System.Web.Http.Dependencies;
using ABC.Infrastructure.ActivityBase;


namespace ABC.Infrastructure.Web.Controllers
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
            if ( serviceType == typeof( DevicesController ) )
                return new DevicesController( ActivityService.ActivitySystem );
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