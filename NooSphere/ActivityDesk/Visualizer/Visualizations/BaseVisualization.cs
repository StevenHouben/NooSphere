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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Surface.Presentation.Controls;

namespace ActivityDesk.Visualizer.Visualization
{
    public abstract class BaseVisualization : TagVisualization
    {
        public BaseVisualization(){}
        public virtual void Enter() { }
        public virtual void Leave() { }
        public event LockedEventHandler Locked = null;
        protected virtual void OnLocked()
        {
            if (Locked != null)
                Locked(this, new LockedEventArgs(VisualizedTag.Value.ToString()));
        }
    }
    public delegate void LockedEventHandler(Object sender, LockedEventArgs e);
    public class LockedEventArgs
    {
        public string VisualizedTag { get; set; }
        public LockedEventArgs(string tag)
        {
            VisualizedTag = tag;
        }
    }
}
