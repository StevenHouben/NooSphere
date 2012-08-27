﻿/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Collections;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

namespace ActivityUI
{
    /// <summary>
    /// Converter writen by Rudi
    /// </summary>
    public class IconToAvgColorBrushConverter : IValueConverter
    {
        #region IValueConverter Members

        /// <summary>
        /// The converter method
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Verify if the value is null, if it is return a transparent brush
            if (value == null)
            {
                return Brushes.Transparent;
            }

            // Instanciate a MemoryStream to acomodate the bitmap
            var stream = new MemoryStream();

            // verify if the value is from the espected Type
            if (value is RenderTargetBitmap)
            {
                // Load the rendered visual element from the render to a BitmapFrame
                BitmapFrame frame = BitmapFrame.Create(value as RenderTargetBitmap);
                // Instanciate the Encoder for the image and add the frame to it
                var e = new BmpBitmapEncoder();
                e.Frames.Add(frame);
                // Save the data to the stream object
                e.Save(stream);
            }

            try
            {
                using (var bitmap = new Bitmap(stream))
                {
                    // Create an Array to acomodate the colors from the image
                    var colors = new ArrayList();

                    // simple loop to load each color to the array
                    for (int x = 0; x < bitmap.Width; x++)
                    {
                        for (int y = 0; y < bitmap.Height; y++)
                        {
                            Color pixel = bitmap.GetPixel(x, y);
                            // verify if the color is transparent because a color of #00000000 would darker the brush
                            if (pixel.A > 0x00)
                                colors.Add(pixel);
                        }
                    }

                    // Using linq to get the average color RGB bytes
                    var r = (byte) Math.Floor(colors.Cast<Color>().Average(c => c.R));
                    var g = (byte) Math.Floor(colors.Cast<Color>().Average(c => c.G));
                    var b = (byte) Math.Floor(colors.Cast<Color>().Average(c => c.B));

                    // Instanciate and initialize the LinearGradientBrush that will be returned as the result of the operation
                    var brush = new LinearGradientBrush();
                    brush.EndPoint = new Point(0.5, 1.0);
                    brush.StartPoint = new Point(0.5, 0.0);
                    brush.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(0x00, r, g, b), 0.00));
                    brush.GradientStops.Add(new GradientStop(System.Windows.Media.Color.FromArgb(0xFF, r, g, b), 1.00));
                    return brush;
                }
            }
            catch (Exception)
            {
                // If any error occours return a Tranparent brush
                return Brushes.Transparent;
            }
        }

        /// <summary>
        /// Just ignored cause it is not supposed to do anything
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }
}