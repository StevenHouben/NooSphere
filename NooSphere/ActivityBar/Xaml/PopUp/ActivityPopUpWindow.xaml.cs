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
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using NooSphere.Core.ActivityModel;
using System.Windows.Interop;

namespace ActivityUI.PopUp
{
    /// <summary>
    /// Interaction logic for PopUpWindow.xaml
    /// </summary>
    public partial class PopUpWindow : Window
    {

        private ActivityBar taskbar;
        private Activity activity;
        public PopUpWindow(ActivityBar bar)
        {
            InitializeComponent();
            this.Topmost = true;

            this.MinHeight = this.MaxHeight = this.Height;
            this.MinWidth = this.MaxWidth = this.Width;
            this.taskbar = bar;
        }

        [DllImport("user32.dll")]
        static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        private const int GWL_STYLE = -16;

        private const uint WS_SYSMENU = 0x80000;

        protected override void OnSourceInitialized(EventArgs e)
        {
            IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE,
                GetWindowLong(hwnd, GWL_STYLE) & (0xFFFFFFFF ^ WS_SYSMENU));

            base.OnSourceInitialized(e);
        }
        public void Show(Activity act,int offset)
        {
            this.activity = act;
            this.Left = offset;
            this.Top = taskbar.Height+5;

            this.txtActivity.Text = act.Name;
            this.txtInfo.Text = act.Id.ToString() ;

            this.Show();
        }
        private void btnDone_Click(object sender, RoutedEventArgs e)
        {
            activity.Name = txtActivity.Text;
            //other stuff
            taskbar.EditActivity(activity);
            this.Hide();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            taskbar.DeleteActivity();
            this.Hide();
        }
    }
}
