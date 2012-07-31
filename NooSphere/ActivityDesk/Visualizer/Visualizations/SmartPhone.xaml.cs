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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;

namespace ActivityDesk.Visualizer.Visualizations
{
    /// <summary>
    /// Interaction logic for SmartPhone.xaml
    /// </summary>
    public partial class SmartPhone : TagVisualization
    {
        public SmartPhone()
        {
            InitializeComponent();
        }

        private void SmartPhone_Loaded(object sender, RoutedEventArgs e)
        {
            //TODO: customize SmartPhone's UI based on this.VisualizedTag here
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
           
        }
    }
}
