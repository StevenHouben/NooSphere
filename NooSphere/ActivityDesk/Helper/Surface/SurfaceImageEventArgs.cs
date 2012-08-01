using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;

namespace ActivityDesk.Helper.Surface
{
    public class SurfaceImageEventArgs : EventArgs
    {
        public Image<Gray, byte> Image { get; private set; }

        public SurfaceImageEventArgs(Image<Gray, byte> image)
        {
            Image = image;
        }
    }
}
