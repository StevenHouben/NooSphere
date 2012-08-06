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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Data;

namespace ActivityUI
{
    /// <summary>
    /// Class writen by Raul
    /// </summary>
    public class Win7ColorHotTrackExtension
    {
        #region HotTracking
        #region AttachedProperty Declaration
        /// <summary>
        /// Declaration of the attached property that we will use to apply the effect to the desired ContentControl
        /// *IT MUST BE A CONTENTCONTROL IN ORDER TO WORK*
        /// </summary>
        public static readonly DependencyProperty ApplyHotTrackingProperty =
            DependencyProperty.RegisterAttached("ApplyHotTracking",
                        typeof(Boolean),
                        typeof(Win7ColorHotTrackExtension),
                        new UIPropertyMetadata(
                            new PropertyChangedCallback(
                                (sender, e) =>
                                {
                                    // If a new value is defined it either wire up the needed events or just unwire the unneeded event
                                    if ((bool)e.NewValue)
                                    {
                                        (sender as FrameworkElement).PreviewMouseMove += new System.Windows.Input.MouseEventHandler(Win7ColorHotTrackExtenssion_PreviewMouseMove);
                                        (sender as FrameworkElement).Loaded += new RoutedEventHandler(Win7ColorHotTrackExtenssion_Loaded);
                                    }
                                    else
                                        (sender as FrameworkElement).PreviewMouseMove -= Win7ColorHotTrackExtenssion_PreviewMouseMove;
                                })));
        #endregion

        #region EventHandlers
        /// <summary>
        /// Load event
        /// </summary>
        /// <param name="sender">The control sender of the change</param>
        /// <param name="e">the parameters for the RoutedEvent</param>
        static void Win7ColorHotTrackExtenssion_Loaded(object sender, RoutedEventArgs e)
        {
            // As stated before, it will not work if the element is not a content control so an exception is thrown
            if (!(sender is ContentControl))
                throw new NotSupportedException("This attached property is just supported by an ContentControl");

            var control = (ContentControl)sender;

            // verify if any data is binded to the Tag property, because if it is, we dont want to lose it
            if (control.GetValue(FrameworkElement.TagProperty) == null)
            {
                // Instantiate and Invalidate the VisualBrush needed for the analisys of the content
                VisualBrush b = new VisualBrush();

                b.SetValue(VisualBrush.VisualProperty, ((StackPanel)control.Content).Children[0]);
                control.InvalidateVisual();

                // if the control has no visual (with a height lesser or equal to zero) 
                // we dont need to perform any action, couse the result will be a transparent brush anyway
                if ((control as FrameworkElement).ActualHeight <= 0)
                    return;

                // Render the visual of the element to an bitmap with the RenderTargetBitmap class
                RenderTargetBitmap RenderBmp = null;
                try
                {
                    RenderBmp = new RenderTargetBitmap(
                        (int)(((StackPanel)control.Content).Children[0] as FrameworkElement).Width,
                        (int)(((StackPanel)control.Content).Children[0] as FrameworkElement).Height,
                        96,
                        96,
                        PixelFormats.Pbgra32);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }

                RenderBmp.Render(b.Visual);

                // Set the value to the Tag property
                control.SetValue(FrameworkElement.TagProperty, RenderBmp);

                control.Background = Brushes.LightBlue;
                // Instanciate and initialize a Binding element to handle the new tag property
                Binding bindBG = new Binding("Tag");
                bindBG.Source = control;
                // Define the converter that will be used to handle the transformation from an image to average color
                bindBG.Converter = new IconToAvgColorBrushConverter();

                // Set the binding to the Background property
                control.SetBinding(ContentControl.BackgroundProperty, bindBG);
                control.SetValue(ContentControl.BorderBrushProperty, new SolidColorBrush(Colors.White));

            }
        }

        /// <summary>
        /// Handles the mouse move to rotate the LinearGradientBrush used to fill the background of our control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Win7ColorHotTrackExtenssion_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            // As already said the sender must be a Content Control
            if (!(sender is ContentControl))
                return;

            ContentControl element = sender as ContentControl;

            // if the Brush is not a linearGradientBrush we dont need to do anything so just returns
            if (!(element.GetValue(ContentControl.BackgroundProperty) is LinearGradientBrush))
            {
                //element.SetValue(ContentControl.BackgroundProperty, new LinearGradientBrush());
                return;
            }

            // Get the brush
            LinearGradientBrush b = element.GetValue(ContentControl.BackgroundProperty) as LinearGradientBrush;

            // Get the ActualWidth of the sender
            Double refZeroX = (double)element.GetValue(ContentControl.ActualWidthProperty);

            // Get the new poit for the StartPoint and EndPoint of the Gradient
            System.Windows.Point p = new System.Windows.Point(e.GetPosition(element).X / refZeroX, 1);

            // Set the new values
            b.StartPoint = new System.Windows.Point(1 - p.X, 0);
            b.EndPoint = p;
        }
        #endregion

        #region AttachedProperty Accessors
        /// <summary>
        /// Getter of the AttachedProperty
        /// </summary>
        /// <param name="sender">Object that has the property attached</param>
        /// <returns>The current value of the property</returns>
        public static Boolean GetApplyHotTracking(DependencyObject sender)
        {
            return (Boolean)sender.GetValue(Win7ColorHotTrackExtension.ApplyHotTrackingProperty);
        }

        /// <summary>
        /// Setter of the AttachedProperty
        /// </summary>
        /// <param name="sender">Object that has the property attached</param>
        /// <param name="value">The new value of the property</param>
        public static void SetApplyHotTracking(DependencyObject sender, Boolean value)
        {
            sender.SetValue(Win7ColorHotTrackExtension.ApplyHotTrackingProperty, value);
        }
        #endregion
        #endregion
    }

}
