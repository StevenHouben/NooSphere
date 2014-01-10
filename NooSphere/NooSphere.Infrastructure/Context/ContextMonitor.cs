using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace ABC.Infrastructure.Context
{
    public class ContextMonitor
    {
        #region Properties

        public Dictionary<Guid, IContextService> Services { get; set; }
        public List<Task> Tasks { get; set; }
        public bool Running { get; set; }

        #endregion


        #region Private Members

        readonly CancellationTokenSource _cancellation = new CancellationTokenSource();

        #endregion


        #region Events

        public event DataReceivedHandler DataReceived = null;
        public event EventHandler Started = null;
        public event EventHandler Stopped = null;

        #endregion


        #region Constructor

        public ContextMonitor( Dictionary<Guid, IContextService> services = null )
        {
            Services = services ?? new Dictionary<Guid, IContextService>();

            Running = false;
        }

        public void Start()
        {
            foreach ( var contextService in Services.Values )
            {
                var service = contextService;
                Task.Factory.StartNew( () =>
                {
                    service.DataReceived += ContextServiceDataReceived;
                    service.Start();
                }, _cancellation.Token );
            }
            if ( Started != null )
                Started( this, new EventArgs() );
        }

        public void Stop()
        {
            foreach ( var contextService in Services.Values )
            {
                contextService.DataReceived -= ContextServiceDataReceived;
                contextService.Stop();
            }
            _cancellation.Cancel();
            if ( Stopped != null )
                Stopped( this, new EventArgs() );
        }

        public void AddContextService( IContextService service )
        {
            Services.Add( service.Id, service );
        }

        public void RemoveContextService( Guid id )
        {
            Services.Remove( id );
        }

        #endregion


        #region Event Handler

        void ContextServiceDataReceived( object sender, DataEventArgs e )
        {
            if ( DataReceived != null )
                DataReceived( sender, e );
        }

        #endregion
    }
}