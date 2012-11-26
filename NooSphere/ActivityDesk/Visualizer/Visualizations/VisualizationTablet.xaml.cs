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
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Shapes;
using BaseVis;
using ActivityDesk.Visualizer.Visualization;

namespace BaseVis
{
    /// <summary>
    /// A simple "glow" visualization. It exposes a pair of methods, Enter() and Leave(),
    /// which you can call to indicate that it has entered or left other UI elements.
    /// </summary>
    /// <remarks>
    /// This implementation causes the visualization to fade out when it enters another
    /// object, and fade back in when it leaves. We use a count rather than a boolean
    /// toggle because it's possible to enter more than one item at a time (for example,
    /// entering a child doesn't equal leaving the parent).
    /// </remarks>
    public partial class VisualizationTablet : BaseVisualization
    {
        private int enterCount;

        /// <summary>
        /// Constructor.
        /// </summary>
        public VisualizationTablet()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Call to indicate that the visualization has entered another UI element.
        /// </summary>
        public override void Enter()
        {
            ++enterCount;
            if (enterCount == 1)
            {
                Animate(0.5, 1.0, 0.4, 0.0, 1.0);
            }
        }

        /// <summary>
        /// Call to indicate that the visualization has left another UI element.
        /// </summary>
        public override void Leave()
        {
            Debug.Assert(enterCount > 0);
            --enterCount;
            if (enterCount < 1)
            {
                Animate(0.2, 0.4, 1.0, 1.0, 0.0);
            }
        }

        /// <summary>
        /// Change the logical highlight "level" of the visualization (0 = totally
        /// un-highlighted, 1 = totally highlighted).
        /// </summary>
        /// <remarks>
        /// The implementation here is to affect transparency and size. "Highlighted"
        /// = opaque and large, "un-highlighted" = transparent and small.
        /// </remarks>
        /// <param name="seconds"></param>
        /// <param name="fromLevel"></param>
        /// <param name="toLevel"></param>
        /// <param name="accelerationRatio"></param>
        /// <param name="decelerationRatio"></param>
        private void Animate(
            double seconds,
            double fromLevel,
            double toLevel,
            double accelerationRatio,
            double decelerationRatio)
        {
            DoubleAnimation animation = new DoubleAnimation(
                fromLevel,
                toLevel,
                new Duration(TimeSpan.FromSeconds(seconds)));
            animation.AccelerationRatio = accelerationRatio;
            animation.DecelerationRatio = decelerationRatio;
        }

        private void SurfaceButton_Click(object sender, RoutedEventArgs e)
        {
            OnLocked();   
        }

        protected override void OnLocked()
        {
            base.OnLocked();
        }
    }
}