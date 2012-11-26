using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Ink;

namespace ActivityDesk
{
	/// <summary>
	/// Interaction logic for Note.xaml
	/// </summary>
	public partial class Note : ScatterViewItem
	{
        public event EventHandler Save = null;
        public event EventHandler Close = null;

		public Note()
		{
			this.InitializeComponent();
		}

        private SurfaceButton reset;
        private SurfaceInkCanvas painter;
        public override void OnApplyTemplate()
        {
            DependencyObject d = GetTemplateChild("btnReset");
            if (d != null)
            {
                reset = d as SurfaceButton;
                reset.Click += new RoutedEventHandler(btnReset_Click);
            }

            DependencyObject e = GetTemplateChild("Painter");
            if (e != null)
            {
                painter = e as SurfaceInkCanvas;
                // Set up the DrawingAttributes for the pen.
                var inkDA = new DrawingAttributes();
                inkDA.Color = Colors.Black;
                inkDA.Height = 1;
                inkDA.Width = 1;
                inkDA.FitToCurve = false;
                painter.UsesTouchShape = false;

                painter.DefaultDrawingAttributes = inkDA;
            }  


            base.OnApplyTemplate();
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            painter.Strokes.Clear();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            //todo
        }

        private void btnClose_PreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (Close != null)
                Close(this, new EventArgs());
        }
	}
}