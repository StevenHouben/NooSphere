using System.Diagnostics;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace ActivityDesk.Visualizer
{
    /// <summary>
    /// Handler for TagVisualizationEnterLeaveEventArgs events.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    public delegate void TagVisualizationEnterLeaveEventHandler(
        object sender,
        TagVisualizationEnterLeaveEventArgs e);


    /// <summary>
    /// Event arguments for TagVisualizationEvents.
    /// </summary>
    public class TagVisualizationEnterLeaveEventArgs : RoutedEventArgs
    {
        private readonly TagVisualization visualization;
        private readonly bool isFirstOrLast;

        /// <summary>
        /// Gets the visualization associated with the event.
        /// </summary>
        public TagVisualization Visualization
        {
            get { return visualization; }
        }

        /// <summary>
        /// Gets whether or not the visualization associated with this event
        /// is the first (if entering) or last (if leaving).
        /// </summary>
        public bool IsFirstOrLast
        {
            get { return isFirstOrLast; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="visualization"></param>
        /// <param name="routedEvent"></param>
        internal TagVisualizationEnterLeaveEventArgs(
            TagVisualization visualization,
            bool isFirstOrLast,
            RoutedEvent routedEvent)
            : base(routedEvent)
        {
            Debug.Assert(visualization != null);
            this.visualization = visualization;
            this.isFirstOrLast = isFirstOrLast;
        }
    }
}