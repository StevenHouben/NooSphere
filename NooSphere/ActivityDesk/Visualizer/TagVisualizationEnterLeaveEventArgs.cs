/// <licence>
/// 
/// (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)
/// 
/// Pervasive Interaction Technology Laboratory (pIT lab)
/// IT University of Copenhagen
///
/// This library is free software; you can redistribute it and/or 
/// modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
/// as published by the Free Software Foundation. Check 
/// http://www.gnu.org/licenses/gpl.html for details.
/// 
/// </licence>

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