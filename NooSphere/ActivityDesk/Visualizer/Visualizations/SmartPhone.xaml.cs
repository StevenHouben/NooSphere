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
    }
}
