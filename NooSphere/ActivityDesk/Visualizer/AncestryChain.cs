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
    /// Utility class that walks up the logical and visual trees from a dependency
    /// object, building a unique list of objects along the way and stopping when
    /// a specified condition is met.
    /// </summary>
    public class AncestryChain : IEnumerable<DependencyObject>
    {
        private readonly List<DependencyObject> chain = new List<DependencyObject>(2);
        private readonly bool isComplete;

        /// <summary>
        /// Gets whether the stop condition was met when the chain was constructed.
        /// </summary>
        public bool IsComplete
        {
            get { return isComplete; }
        }

        /// <summary>
        /// Constructs a new chain over the specified element.
        /// </summary>
        /// <param name="elementOver"></param>
        /// <param name="stopCondition"></param>
        public AncestryChain(DependencyObject elementOver, Predicate<DependencyObject> stopCondition)
        {
            Debug.Assert(elementOver != null);
            isComplete = RecurseParent(elementOver, stopCondition, chain);
        }

        /// <summary>
        /// Finds items in another chain that this one doesn't contain. Returns null
        /// if there's no difference.
        /// </summary>
        /// <param name="newChain"></param>
        public IEnumerable<DependencyObject> FindDiff(
            AncestryChain newChain)
        {
            Debug.Assert(newChain != null);

            List<DependencyObject> diff = null;
            foreach (DependencyObject obj in newChain.chain)
            {
                if (!chain.Contains(obj))
                {
                    if (diff == null)
                    {
                        int capacity = Math.Max(1, newChain.chain.Count - chain.Count);
                        diff = new List<DependencyObject>(capacity); ;
                    }
                    diff.Add(obj);
                }
            }
            return diff;
        }

        /// <summary>
        /// Recurses up the parent tree until arriving at an object that meets the stop condition.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="stopCondition"></param>
        /// <param name="chain"></param>
        /// <returns>True if the stop condition was met, false otherwise.</returns>
        private static bool RecurseParent(
            DependencyObject element,
            Predicate<DependencyObject> stopCondition,
            List<DependencyObject> chain)
        {
            Debug.Assert(element != null);
            Debug.Assert(chain != null);

            if (chain.Contains(element))
            {
                // Duplicate element, stop here.
                return false;
            }

            chain.Add(element);
            if ((stopCondition != null) && stopCondition(element))
            {
                return true;
            }

            // Try the visual tree first
            bool metStopCondition = false;
            DependencyObject visualParent = VisualTreeHelper.GetParent(element);
            if (visualParent != null)
            {
                metStopCondition = RecurseParent(visualParent, stopCondition, chain);
            }

            // Next, try the logical tree, if it's not found yet
            if (!metStopCondition)
            {
                DependencyObject logicalParent = LogicalTreeHelper.GetParent(element);
                if ((logicalParent != null) && (logicalParent != visualParent))
                {
                    metStopCondition = RecurseParent(logicalParent, stopCondition, chain);
                }
            }

            return metStopCondition;
        }

        #region IEnumerable<DependencyObject> Members

        public IEnumerator<DependencyObject> GetEnumerator()
        {
            return chain.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return chain.GetEnumerator();
        }

        #endregion
    }
}
