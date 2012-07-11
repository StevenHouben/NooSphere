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
