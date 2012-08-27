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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls;

namespace ActivityDesk.Visualizer
{
    /// <summary>
    /// Does tracking and hit-testing of TagVisualization positions to raise
    /// events such as enter, leave, etc.
    /// </summary>
    /// <remarks>
    /// Note that using this class is computationally expensive. When you enable
    /// event tracking with this class, it will cause potentially large numbers
    /// of expensive hit-tests. Use with caution.
    /// </remarks>
     public static class TagVisualizerEvents
    {
        #region Data members
        private static readonly TagVisualization[] emptyVisualizationList = new TagVisualization[0];

        // keep track of active visualizers; the boolean flag indicates if auto-synchronize is on
        private static readonly Dictionary<TagVisualizer, bool> visualizers = new Dictionary<TagVisualizer,bool>();

        // keep track of ancestry chains for active visualizations
        private static readonly Dictionary<TagVisualization, AncestryChain> visualizationChains
            = new Dictionary<TagVisualization, AncestryChain>();

        // keep track of the visualizations that are over specific dependency objects
        private static readonly Dictionary<DependencyObject, List<TagVisualization>> elementVisualizations
            = new Dictionary<DependencyObject,List<TagVisualization>>();

        // keep track of how many visualizers have auto-synchronize turned on
        private static int autoSynchronizeCount;
        #endregion Data members


        #region Public events
        /// <summary>
        /// Declares the VisualizationEnter event, which fires when a visualization enters another
        /// dependency object.
        /// </summary>
        public static readonly RoutedEvent VisualizationEnterEvent = EventManager.RegisterRoutedEvent(
            "VisualizationEnter",
            RoutingStrategy.Direct,
            typeof(TagVisualizationEnterLeaveEventHandler),
            typeof(TagVisualizerEvents));

        /// <summary>
        /// Adds a handler for the VisualizationEnter event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="handler"></param>
        public static void AddVisualizationEnterHandler(
            DependencyObject element,
            TagVisualizationEnterLeaveEventHandler handler)
        {
            AddHandler(element, VisualizationEnterEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the VisualizationEnter event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="handler"></param>
        public static void RemoveVisualizationEnterHandler(
            DependencyObject element,
            TagVisualizationEnterLeaveEventHandler handler)
        {
            RemoveHandler(element, VisualizationEnterEvent, handler);
        }


        /// <summary>
        /// Declares the VisualizationLeave event, which fires when a visualization leaves another
        /// dependency object.
        /// </summary>
        public static readonly RoutedEvent VisualizationLeaveEvent = EventManager.RegisterRoutedEvent(
            "VisualizationLeave",
            RoutingStrategy.Direct,
            typeof(TagVisualizationEnterLeaveEventHandler),
            typeof(TagVisualizerEvents));

        /// <summary>
        /// Adds a handler for the VisualizationLeave event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="handler"></param>
        public static void AddVisualizationLeaveHandler(
            DependencyObject element,
            TagVisualizationEnterLeaveEventHandler handler)
        {
            AddHandler(element, VisualizationLeaveEvent, handler);
        }

        /// <summary>
        /// Removes a handler for the VisualizationLeave event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="handler"></param>
        public static void RemoveVisualizationLeaveHandler(
            DependencyObject element,
            TagVisualizationEnterLeaveEventHandler handler)
        {
            RemoveHandler(element, VisualizationLeaveEvent, handler);
        }
        #endregion Public events


        #region Public properties
        /// <summary>
        /// Attached property that controls the activity of TagVisualizerEvents.
        /// Set this property on a TagVisualizer to control event tracking.
        /// </summary>
        public static readonly DependencyProperty ModeProperty = DependencyProperty.RegisterAttached(
            "Mode",
            typeof(TagVisualizerSynchronizationMode),
            typeof(TagVisualizerEvents),
            new FrameworkPropertyMetadata(TagVisualizerSynchronizationMode.Off, OnModeChanged),
            ValidateMode);

        /// <summary>
        /// Sets the value of the Mode attached property.
        /// </summary>
        /// <param name="visualizer"></param>
        /// <param name="mode"></param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification="Mode property is specific to TagVisualizer")]
        public static void SetMode(TagVisualizer visualizer, TagVisualizerSynchronizationMode mode)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }
            visualizer.SetValue(ModeProperty, mode);
        }

        /// <summary>
        /// Gets the value of the Mode attached property.
        /// </summary>
        /// <param name="visualizer"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters",
            Justification = "Mode property is specific to TagVisualizer")]
        public static TagVisualizerSynchronizationMode GetMode(TagVisualizer visualizer)
        {
            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }
            return (TagVisualizerSynchronizationMode)visualizer.GetValue(ModeProperty);
        }


        /// <summary>
        /// Private property key for setting the IsAnyVisualizationOver attached property.
        /// </summary>
        private static readonly DependencyPropertyKey IsAnyVisualizationOverPropertyKey
            = DependencyProperty.RegisterAttachedReadOnly(
                "IsAnyVisualizationOver",
                typeof(bool),
                typeof(TagVisualizerEvents),
                new FrameworkPropertyMetadata(false));

        /// <summary>
        /// Attached readonly property that indicates whether any visualizations
        /// are over an element.
        /// </summary>
        public static readonly DependencyProperty IsAnyVisualizationOverProperty
            = IsAnyVisualizationOverPropertyKey.DependencyProperty;

        /// <summary>
        /// Sets the value of the IsAnyVisualizationOver attached property.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="isOver"></param>
        private static void SetIsAnyVisualizationOver(DependencyObject element, bool isOver)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            element.SetValue(IsAnyVisualizationOverPropertyKey, isOver);
        }

        /// <summary>
        /// Gets the value of the IsAnyVisualizationOver attached property.
        /// </summary>
        /// <remarks>
        /// The element should be of type TagVisualizer.
        /// </remarks>
        /// <param name="element"></param>
        /// <returns></returns>
        public static bool GetIsAnyVisualizationOver(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            return (bool)element.GetValue(IsAnyVisualizationOverProperty);
        }
        #endregion Public properties


        /// <summary>
        /// Gets the visualizations that are over the specified element.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static IEnumerable<TagVisualization> GetVisualizationsOver(DependencyObject element)
        {
            List<TagVisualization> visualizations;
            if (elementVisualizations.TryGetValue(element, out visualizations))
            {
                return visualizations;
            }
            else
            {
                return emptyVisualizationList;
            }
        }

        /// <summary>
        /// Synchronizes the status of all visualizations in tracked visualizers, firing events as
        /// appropriate. Note that this is done automatically for visualizers that have
        /// TagVisualizerEvents.Mode set to TagVisualizerSynchronizationMode.Auto, so in that mode
        /// you only need to call this if you move an item and want to get feedback right away
        /// rather than asynchronously.  For visualizers set to TagVisualizerSynchronizationMode.Manual,
        /// you need to call this any time anything other than a visualization moves.
        /// </summary>
        public static void Synchronize()
        {
            Synchronize(false);
        }


        #region Private methods
        /// <summary>
        /// Here when the Mode property is changed on a dependency object.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="e"></param>
        private static void OnModeChanged(
            DependencyObject obj,
            DependencyPropertyChangedEventArgs e)
        {
            TagVisualizer visualizer = obj as TagVisualizer;
            if (visualizer == null)
            {
                // This property is only allowed to be set on TagVisualizer
                throw new ArgumentException("TagVisualizerEvents.Mode is only valid for TagVisualizer");
            }
            else
            {
                OnVisualizerModeChanged(
                    visualizer,
                    (TagVisualizerSynchronizationMode)e.OldValue,
                    (TagVisualizerSynchronizationMode)e.NewValue);
            }
        }


        /// <summary>
        /// Here when the Mode property on a TagVisualizer changes.
        /// </summary>
        /// <param name="visualizer"></param>
        /// <param name="oldMode"></param>
        /// <param name="newMode"></param>
        private static void OnVisualizerModeChanged(
            TagVisualizer visualizer,
            TagVisualizerSynchronizationMode oldMode,
            TagVisualizerSynchronizationMode newMode)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(oldMode != newMode);

            // Add or remove visualizer if necessary
            bool oldIsActive = oldMode != TagVisualizerSynchronizationMode.Off;
            bool newIsActive = newMode != TagVisualizerSynchronizationMode.Off;
            if (!oldIsActive && newIsActive)
            {
                AddVisualizer(visualizer);
            }
            else if (oldIsActive && !newIsActive)
            {
                RemoveVisualizer(visualizer);
            }

            // Is the visualizer active?
            if (newIsActive)
            {
                bool oldAutoSynchronize = oldMode == TagVisualizerSynchronizationMode.Auto;
                bool newAutoSynchronize = newMode == TagVisualizerSynchronizationMode.Auto;
                if (!oldAutoSynchronize && newAutoSynchronize)
                {
                    BeginAutoSynchronize(visualizer);
                }
                else if (oldAutoSynchronize && !newAutoSynchronize)
                {
                    EndAutoSynchronize(visualizer);
                }
            }
        }

        /// <summary>
        /// Registers a new visualizer for event processing.
        /// </summary>
        /// <param name="visualizer"></param>
        private static void AddVisualizer(TagVisualizer visualizer)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(!visualizers.ContainsKey(visualizer), "Trying to add visualizer that's already registered");

            visualizers.Add(visualizer, false);

            // Register with the visualizer so that whenever a visualization moves, is added,
            // or is removed, it can be synchronized.
            visualizer.VisualizationMoved += OnSynchronizeVisualization;
            visualizer.VisualizationAdded += OnSynchronizeVisualization;
            visualizer.VisualizationRemoved += OnRemoveVisualization;
        }

        /// <summary>
        /// Unregisters a previous visualizer from event processing.
        /// </summary>
        /// <param name="visualizer"></param>
        private static void RemoveVisualizer(TagVisualizer visualizer)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(visualizers.ContainsKey(visualizer), "Trying to remove unknown visualizer");

            // Unregister from  the visualizer.
            visualizer.VisualizationMoved -= OnSynchronizeVisualization;
            visualizer.VisualizationAdded -= OnSynchronizeVisualization;
            visualizer.VisualizationRemoved -= OnRemoveVisualization;

            // Turn off auto-synchronize, if it's on
            if (visualizers[visualizer])
            {
                EndAutoSynchronize(visualizer);
            }

            visualizers.Remove(visualizer);
        }

        /// <summary>
        /// Turns on auto-synchronize for the specified visualizer.
        /// </summary>
        /// <param name="visualizer"></param>
        private static void BeginAutoSynchronize(TagVisualizer visualizer)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(visualizers.ContainsKey(visualizer), "Unknown visualizer");
            Debug.Assert(!visualizers[visualizer], "Auto-synchronize was already turned on");

            // Make sure that this is the visualizer's thread, to be sure
            // to get the correct input manager
            visualizer.VerifyAccess();

            // Mark it as being tracked
            ++autoSynchronizeCount;
            visualizers[visualizer] = true;

            // If it's the first one, activate tracking
            if (autoSynchronizeCount == 1)
            {
                InputManager.Current.HitTestInvalidatedAsync += OnHitTestInvalidatedAsync;
            }
        }

        /// <summary>
        /// Turns off auto-synchronize for the specified visualizer.
        /// </summary>
        /// <param name="visualizer"></param>
        private static void EndAutoSynchronize(TagVisualizer visualizer)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(visualizers.ContainsKey(visualizer), "Unknown visualizer");
            Debug.Assert(visualizers[visualizer], "Auto-synchronize was already turned off");
            Debug.Assert(autoSynchronizeCount > 0, "Auto-synchronize count is corrupt");

            // Make sure that this is the visualizer's thread, to be sure
            // to get the correct input manager
            visualizer.VerifyAccess();

            // Mark it as being un-tracked
            --autoSynchronizeCount;
            visualizers[visualizer] = false;

            // If it's the last one, stop tracking
            if (autoSynchronizeCount < 1)
            {
                InputManager.Current.HitTestInvalidatedAsync -= OnHitTestInvalidatedAsync;
            }
        }

        /// <summary>
        /// Called when the HitTestInvalidatedAsync event on the event manager fires.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnHitTestInvalidatedAsync(object sender, EventArgs e)
        {
            Synchronize(true);
        }

        /// <summary>
        /// Synchronizes all visualizations.
        /// </summary>
        /// <param name="forceAll">True to force all to synchronize,
        /// false to only synchronize the ones that aren't auto-synchronizing.</param>
        private static void Synchronize(bool forceAll)
        {
            foreach (KeyValuePair<TagVisualizer, bool> pair in visualizers)
            {
                if (forceAll || !pair.Value)
                {
                    // The visualizer isn't auto-updating (or a sync is being forced),
                    // so it's necessary to synchronize all its visualizations.
                    pair.Key.VerifyAccess();
                    foreach (TagVisualization visualization in pair.Key.ActiveVisualizations)
                    {
                        SynchronizeVisualization(pair.Key, visualization);
                    }
                }
            }
        }

        /// <summary>
        /// Here when it's time to add or synchronize the state of a visualization.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnSynchronizeVisualization(object sender, TagVisualizerEventArgs e)
        {
            SynchronizeVisualization((TagVisualizer)sender, e.TagVisualization);
        }

        /// <summary>
        /// Synchronizes the specified visualization.
        /// </summary>
        /// <param name="visualizer"></param>
        /// <param name="visualization"></param>
        private static void SynchronizeVisualization(TagVisualizer visualizer, TagVisualization visualization)
        {
            Debug.Assert(visualizer != null);
            Debug.Assert(visualization != null);
            Debug.Assert(visualizer.ActiveVisualizations.Contains(visualization));

            // Find the element that the visualization is over
            DependencyObject elementHit = VisualizerHitTester.HitTest(visualizer, visualization.Center) ?? visualizer;

            // Get its current ancestry chain
            Predicate<DependencyObject> isVisualizer = delegate(DependencyObject obj) { return obj == visualizer; };
            AncestryChain newChain = new AncestryChain(elementHit, isVisualizer);
            Debug.Assert(newChain.IsComplete, "Hit element's ancestry chain didn't reach visualizer");

            // Get its old chain, if any
            AncestryChain oldChain;
            if (visualizationChains.TryGetValue(visualization, out oldChain))
            {
                Debug.Assert(oldChain != null);

                // We have an old chain. Process any adds or removes.
                IEnumerable<DependencyObject> removes = newChain.FindDiff(oldChain);
                if (removes != null)
                {
                    OnVisualizationsLeave(removes, visualization);
                }
                IEnumerable<DependencyObject> adds = oldChain.FindDiff(newChain);
                if (adds != null)
                {
                    OnVisualizationsEnter(adds, visualization);
                }
            }
            else
            {
                // We don't have an old chain. Treat everything as an add.
                OnVisualizationsEnter(newChain, visualization);
            }

            visualizationChains[visualization] = newChain;
        }


        /// <summary>
        /// Here when a visualization is removed from a visualizer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnRemoveVisualization(object sender, TagVisualizerEventArgs e)
        {
            TagVisualization visualization = e.TagVisualization;
            Debug.Assert(visualization != null);
            Debug.Assert(visualizationChains.ContainsKey(visualization));
            AncestryChain chain = visualizationChains[visualization];
            Debug.Assert(chain != null);
            OnVisualizationsLeave(chain, visualization);
        }

        /// <summary>
        /// Here when a visualization enters a list of elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="visualization"></param>
        private static void OnVisualizationsEnter(IEnumerable<DependencyObject> elements, TagVisualization visualization)
        {
            Debug.Assert(elements != null);
            Debug.Assert(visualization != null);

            foreach (DependencyObject element in elements)
            {
                // Update the "visualizations over" lists appropriately
                List<TagVisualization> visualizations;
                if (elementVisualizations.TryGetValue(element, out visualizations))
                {
                    // Already had some visualizations, just add it
                    Debug.Assert(visualizations != null);
                    Debug.Assert(!visualizations.Contains(visualization));
                    visualizations.Add(visualization);
                }
                else
                {
                    // This is the first visualization over it
                    visualizations = new List<TagVisualization>();
                    visualizations.Add(visualization);
                    elementVisualizations.Add(element, visualizations);
                }
            }

            // Set IsAnyVisualizationOver property and raise VisualizationEnter
            // event as appropriate
            foreach (DependencyObject element in elements)
            {
                List<TagVisualization> visualizations = elementVisualizations[element];
                bool isFirst = visualizations.Count == 1;
                if (isFirst)
                {
                    Debug.Assert(!GetIsAnyVisualizationOver(element), "IsAnyVisualizationOver was already true");
                    SetIsAnyVisualizationOver(element, true);
                }
                RaiseEnterLeaveEvent(element, visualization, isFirst, VisualizationEnterEvent);
            }
        }

        /// <summary>
        /// Here when a visualization leaves a list of elements.
        /// </summary>
        /// <param name="elements"></param>
        /// <param name="visualization"></param>
        private static void OnVisualizationsLeave(IEnumerable<DependencyObject> elements, TagVisualization visualization)
        {
            Debug.Assert(elements != null);
            Debug.Assert(visualization != null);

            // Update the "visualizations over" lists appropriately
            foreach (DependencyObject element in elements)
            {
                Debug.Assert(elementVisualizations.ContainsKey(element));
                List<TagVisualization> visualizations = elementVisualizations[element];
                Debug.Assert(visualizations.Contains(visualization));
                visualizations.Remove(visualization);
                if (visualizations.Count == 0)
                {
                    // This is the last remaining visualization over it
                    elementVisualizations.Remove(element);
                }
            }

            // Set IsAnyVisualizationOver property and raise VisualizationLeave
            // event as appropriate
            foreach (DependencyObject element in elements)
            {
                bool isLast = !elementVisualizations.ContainsKey(element);
                if (isLast)
                {
                    Debug.Assert(GetIsAnyVisualizationOver(element), "IsAnyVisualizationOver was already false");
                    SetIsAnyVisualizationOver(element, false);
                }
                RaiseEnterLeaveEvent(element, visualization, isLast, VisualizationLeaveEvent);
            }
        }


        /// <summary>
        /// Raises an enter or leave event on the specified dependency object.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="visualization"></param>
        /// <param name="isFirstOrLast"></param>
        /// <param name="routedEvent"></param>
        private static void RaiseEnterLeaveEvent(
            DependencyObject element,
            TagVisualization visualization,
            bool isFirstOrLast,
            RoutedEvent routedEvent)
        {
            UIElement uiElement = element as UIElement;
            if (uiElement != null)
            {
                uiElement.RaiseEvent(new TagVisualizationEnterLeaveEventArgs(
                    visualization,
                    isFirstOrLast,
                    routedEvent));
            }
            else
            {
                ContentElement contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    contentElement.RaiseEvent(new TagVisualizationEnterLeaveEventArgs(
                        visualization,
                        isFirstOrLast,
                        routedEvent));
                }
            }
        }


        /// <summary>
        /// Validates values set on the Mode attached property.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private static bool ValidateMode(object value)
        {
            if (!(value is TagVisualizerSynchronizationMode))
            {
                return false;
            }
            TagVisualizerSynchronizationMode mode = (TagVisualizerSynchronizationMode)value;
            switch (mode)
            {
                case TagVisualizerSynchronizationMode.Off:
                case TagVisualizerSynchronizationMode.Auto:
                case TagVisualizerSynchronizationMode.Manual:
                    return true;

                default:
                    return false;
            }
        }


        /// <summary>
        /// Adds a specified event handler for a specified attached event.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="routedEvent"></param>
        /// <param name="handler"></param>
        private static void AddHandler(DependencyObject element, RoutedEvent routedEvent, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Debug.Assert(routedEvent != null, "RoutedEvent must not be null");

            UIElement uiElement = element as UIElement;
            if (uiElement != null)
            {
                // this is a UIElement
                uiElement.AddHandler(routedEvent, handler);
            }
            else
            {
                ContentElement contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    // this is a ContentElement
                    contentElement.AddHandler(routedEvent, handler);
                }
                else
                {
                    throw new ArgumentException("Invalid type", "element");
                }
            }
        }

        /// <summary>
        ///  Removes a handler for the given attached event
        /// </summary>
        private static void RemoveHandler(DependencyObject element, RoutedEvent routedEvent, Delegate handler)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            Debug.Assert(routedEvent != null, "RoutedEvent must not be null");

            UIElement uiElement = element as UIElement;
            if (uiElement != null)
            {
                // This is a UIElement
                uiElement.RemoveHandler(routedEvent, handler);
            }
            else
            {
                ContentElement contentElement = element as ContentElement;
                if (contentElement != null)
                {
                    // This is a ContentElement
                    contentElement.RemoveHandler(routedEvent, handler);
                }
                else
                {
                    throw new ArgumentException("Invalid type", "element");
                }
            }
        }
        #endregion Private methods


        #region private class VisualizerHitTester
        /// <summary>
        /// Utility class that does hit-testing that skips IsHitTestVisible=false elements.
        /// </summary>
        /// <remarks>
        /// It's necessary to use this class, rather than just using the simple overload
        /// of VisualTreeHelper.HitTest() that takes a Visual and a point, in order
        /// to skip over items that have IsHitTestVisible equal to false, and also to
        /// skip TagVisualizations themselves.
        /// </remarks>
        private class VisualizerHitTester
        {
            private readonly TagVisualizer visualizer;
            private DependencyObject visualHit;

            public static DependencyObject HitTest(TagVisualizer visualizer, Point point)
            {
                Debug.Assert(visualizer != null);
                VisualizerHitTester tester = new VisualizerHitTester(visualizer);
                VisualTreeHelper.HitTest(
                    visualizer,
                    tester.FilterCallback,
                    tester.ResultCallback,
                    new PointHitTestParameters(point));
                return tester.visualHit;
            }

            /// <summary>
            /// Private constructor.
            /// </summary>
            /// <param name="visualizer"></param>
            private VisualizerHitTester(TagVisualizer visualizer)
            {
                Debug.Assert(visualizer != null);
                this.visualizer = visualizer;
            }

            /// <summary>
            /// Callback for hit test filtering.
            /// </summary>
            /// <param name="target"></param>
            /// <returns></returns>
            private HitTestFilterBehavior FilterCallback(DependencyObject target)
            {
                Visual visual = target as Visual;
                if (visual == null)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }
                UIElement element = visual as UIElement;
                if (element == null)
                {
                    return HitTestFilterBehavior.Continue;
                }
                else if (!element.IsHitTestVisible)
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }

                // The element is a UIElement and is hit-testable.
                // If this element is one of the TagVisualization children
                // of the hit-tester's TagVisualizer, skip it (otherwise
                // it would block hit-testing of items in the visualizer's
                // content). Otherwise, continue.
                TagVisualization visualization = element as TagVisualization;
                if ((visualization != null) && (visualization.Visualizer == visualizer))
                {
                    return HitTestFilterBehavior.ContinueSkipSelfAndChildren;
                }
                else
                {
                    return HitTestFilterBehavior.Continue;
                }
            }

            /// <summary>
            /// Callback for hit test results.
            /// </summary>
            /// <param name="result"></param>
            /// <returns></returns>
            private HitTestResultBehavior ResultCallback(HitTestResult result)
            {
                if (result != null)
                {
                    visualHit = result.VisualHit;
                }
                return HitTestResultBehavior.Stop;
            }
        }
        #endregion private class VisualizerHitTester
    }
}
