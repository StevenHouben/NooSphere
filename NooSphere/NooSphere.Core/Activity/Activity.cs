using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.WindowsAzure.StorageClient;

namespace NooSphere.Core
{
    public class Activity : TableServiceEntity, IEntity, ILifecycle
    {
        #region Constructors
        public Activity()
        {
            IntitializeTableServiceEntity();
        }
        #endregion

        #region Initializers
        private void IntitializeTableServiceEntity()
        {
            PartitionKey = DateTime.UtcNow.ToString("MMddyyyy");
            RowKey = string.Format("{0:10}_{1}",
                                   DateTime.MaxValue.Ticks - DateTime.Now.Ticks,
                                   Guid.NewGuid());
        }
        #endregion

        #region Properties
        /// <summary>
        /// Related files and documents
        /// </summary>
        public List<Resources> Resources { get; set; }

        /// <summary>
        /// Related physical and digital tools
        /// </summary>
        public List<Tool> Tools { get; set; }

        /// <summary>
        /// Actions related to the activity
        /// </summary>
        public List<Action> Actions { get; set; }

        /// <summary>
        /// The owner of the activity
        /// </summary>
        public Actor owner { get; set; }

        /// <summary>
        /// List of primary actors (subject)
        /// </summary>
        public List<Actor> PrimaryActors { get; set; }

        /// <summary>
        /// List os secondary actors (community)
        /// </summary>
        public List<Actor> SecondaryActors { get; set; }

        /// <summary>
        /// History of the activity
        /// </summary>
        /// <remarks>
        /// Should represent a snapshot of the activity
        /// </remarks>
        public List<History> History { get; set; }

        /// <summary>
        /// The setting in which the activity occurs
        /// </summary>
        public Setting Setting { get; set; }
        #endregion

        #region IEntity
        public string Name { get; set; }
        public string ID { get; set; }
        public string Description { get; set; }
        #endregion

        #region ILifecycle
        public bool Pausable { get; set; }
        public bool Resumable { get; set; }
        public bool Startable { get; set; }
        public bool Finishable { get; set; }
        public bool Stoppable { get; set; }

        public void Pause()
        {
            throw new NotImplementedException();
        }
        public void Resume()
        {
            throw new NotImplementedException();
        }
        public void Start()
        {
            throw new NotImplementedException();
        }
        public void Stop()
        {
            throw new NotImplementedException();
        }
        public void Finish()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
