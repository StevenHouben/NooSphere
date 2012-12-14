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
using System.ComponentModel;

namespace ActivityDesk
{
	/// <summary>
	/// Interaction logic for DeviceTumbnail.xaml
	/// </summary>
    public partial class DeviceTumbnail : ScatterViewItem 
	{
        private string _name;
        public string Name
        {
            get{return _name;}
            set
            {
                _name = value;
                if(_label != null)
                    _label.Content = Name;
            }
        }

        private Label _label;
        public override void OnApplyTemplate()
        {
            DependencyObject d = GetTemplateChild("lblName");
            if (d != null)
            {
                _label = d as Label;
                _label.Content = Name;
            }
            base.OnApplyTemplate();
        }

		public DeviceTumbnail()
		{
			this.InitializeComponent();
		} 
    }
}