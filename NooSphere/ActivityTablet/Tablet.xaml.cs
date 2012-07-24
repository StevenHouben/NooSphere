using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Threading;
using System.IO;
using System.Threading;

namespace ActivityTablet
{

    public partial class Tablet : Window
    {

        public Tablet()
        {
            //Initializes design-time components
            InitializeComponent();
        }
      
        private void btnGo_Click(object sender, RoutedEventArgs e)
        {

        }

        private void cancel_Click(object sender, RoutedEventArgs e)
        {
            Environment.Exit(0);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnGo_ManipulationBoundaryFeedback(object sender, ManipulationBoundaryFeedbackEventArgs e)
        {

        }

        private void btnGo_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void btnGo_MouseLeave(object sender, MouseEventArgs e)
        {

        }
    }
}