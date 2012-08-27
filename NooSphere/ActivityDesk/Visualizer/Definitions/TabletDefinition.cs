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
using Microsoft.Surface.Presentation.Input;
using System.Windows;

namespace ActivityDesk.Visualizer.Definitions
{
    public class TabletDefinition : TagVisualizationDefinition
    {
        protected override bool Matches(TagData tag)
        {
            return tag.Value == 204 || tag.Value == 205;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TabletDefinition();
        }
    }
}
