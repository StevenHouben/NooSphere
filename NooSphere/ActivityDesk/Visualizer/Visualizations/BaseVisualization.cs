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
    }
}
