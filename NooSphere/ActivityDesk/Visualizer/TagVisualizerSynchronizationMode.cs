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