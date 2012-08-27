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
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Controls;

namespace ActivityTablet
{
    public class ActivityButton:SurfaceButton
    {
        #region Properties
        public Guid ActivityId { get; set; }
        private Uri image;
        public Uri Image 
        {
            get { return this.image; }
            set
            {
                this.image = value;
                Invalidate();
            }
        }
        
        private string text;
        public string Text
        {
            get { return this.text; }
            set
            {
                this.text = value;
                Invalidate();
            }
        }

        private RenderMode renderMode;
        public RenderMode RenderMode 
        {
            get { return this.renderMode; }
            set 
            {
                this.renderMode = value;
                Invalidate();
            }
 
        }
        #endregion

        #region Constructor
        public ActivityButton()
        {
            this.Image = new Uri("pack://application:,,,/Images/activity.PNG");
            this.Text = "Default";
            this.RenderMode = RenderMode.ImageAndText;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        }
        public ActivityButton(Uri img, string text)
        {
            this.Image = img;
            this.Text = text;
            this.RenderMode = RenderMode.ImageAndText;
            this.VerticalAlignment = System.Windows.VerticalAlignment.Center;
        }
        #endregion

        #region Private Methods
        private void Invalidate()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                this.Content = null;
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;

                if (RenderMode == RenderMode.Image)
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(this.Image);
                    img.Width = img.Height = 50;
                    panel.Children.Add(img);
                    this.Width = 50;
                }
                else if (RenderMode == RenderMode.Text)
                {
                    Label l = new Label();
                    l.Foreground = Brushes.White;
                    l.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    l.Content = this.Text;
                    panel.Children.Add(l);
                    this.Width = 250;
                }
                else
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(this.Image);
                    panel.Children.Add(img);
                    Label l = new Label();
                    l.Foreground = Brushes.White;
                    l.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                    l.Content = this.Text;
                    panel.Children.Add(l);
                    this.Width = 250;
                }
                this.Content = panel;
            }));
        }
        #endregion
    }
    public enum RenderMode
    {
        Image,
        Text,
        ImageAndText
    }
}
