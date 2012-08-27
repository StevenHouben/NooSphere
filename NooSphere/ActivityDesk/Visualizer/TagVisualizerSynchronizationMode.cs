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

namespace ActivityDesk.Visualizer
{
    /// <summary>
    /// Controls the behavior of TagVisualizerEvents.
    /// </summary>
    public enum TagVisualizerSynchronizationMode
    {
        /// <summary>
        /// TagVisualizerEvents processing is not enabled. This is the default.
        /// </summary>
        Off,

        /// <summary>
        /// TagVisualizerEvents processing is enabled. Hit-testing of all
        /// visualizations is automatically synchronized whenever anything
        /// moves.
        /// </summary>
        Auto,

        /// <summary>
        /// TagVisualizerEvents processing is enabled. Hit-testing of
        /// visualizations is only automatically synchronized when the visualizations
        /// themselves move. If anything else moves, hit-testing needs to
        /// be synchronized manually by calling TagVisualizerEvents.Synchronize().
        /// </summary>
        Manual
    }
}