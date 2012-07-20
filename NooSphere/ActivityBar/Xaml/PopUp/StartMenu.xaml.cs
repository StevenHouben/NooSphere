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
using System.Windows.Shapes;


using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.IO;

using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using NooSphere.Core.ActivityModel;
using NooSphere.Platform.Windows.Glass;

namespace ActivityUI
{
	public partial class StartMenu : Window
    {
        #region Members
        private ActivityBar taskbar;
        #endregion

        #region Constructor
        public StartMenu(ActivityBar tBar)
		{
			this.InitializeComponent();
  
            this.ShowInTaskbar = false;

            this.SourceInitialized += new EventHandler(StartMenu_SourceInitialized);
            this.taskbar = tBar;

            this.MinHeight = this.MaxHeight = this.Height;
            this.MinWidth = this.MaxWidth = this.Width;

            this.Top = this.taskbar.Height;
            this.Left = 4;

		}
        #endregion

        #region Window Ajustments
        /// <summary>
        /// Disabled move/box
        /// </summary>
        const int WM_SYSCOMMAND = 0x0112;
        const int SC_MOVE = 0xF010;
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WM_SYSCOMMAND:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == SC_MOVE)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
        #endregion

        #region Methods
        public void SetImage(System.Drawing.Bitmap source)
        {
            this.imgMe.Source = WFormsImageToWPFImage(source).Source;
        }
        public System.Windows.Controls.Image WFormsImageToWPFImage(System.Drawing.Image Old_School_Image)
        {
            MemoryStream ms = new MemoryStream();
            Old_School_Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

            System.Windows.Media.Imaging.BitmapImage bImg = new System.Windows.Media.Imaging.BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();
            System.Windows.Controls.Image WPFImage = new System.Windows.Controls.Image();
            WPFImage.Source = bImg;
            return WPFImage; ;
        }
        public string Username
        {
            get { return (string)lblName.Text; }
            set { lblName.Text = value; }
        }
        #endregion

        #region Glass
        [Flags]
        private enum DwmBlurBehindFlags : uint
        {
            /// <summary>
            /// Indicates a value for fEnable has been specified.
            /// </summary>
            DWM_BB_ENABLE = 0x00000001,

            /// <summary>
            /// Indicates a value for hRgnBlur has been specified.
            /// </summary>
            DWM_BB_BLURREGION = 0x00000002,

            /// <summary>
            /// Indicates a value for fTransitionOnMaximized has been specified.
            /// </summary>
            DWM_BB_TRANSITIONONMAXIMIZED = 0x00000004
        }
        private enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_LAST
        }
        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public DwmBlurBehindFlags dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }
        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);
        private WindowInteropHelper helper;
        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //ApplyDarkGlass();
            ApplyGlass();

        }

        private void InitializeGlass()
        {
            helper = new WindowInteropHelper(this);

        }

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth;      // width of left border that retains its size
            public int cxRightWidth;     // width of right border that retains its size
            public int cyTopHeight;      // height of top border that retains its size
            public int cyBottomHeight;   // height of bottom border that retains its size
        };

        [DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        private void ApplyDarkGlass()
        {
            int val = 1;
            IntPtr result = DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 3, ref val, sizeof(int));
        }
        private MARGINS GetDpiAdjustedMargins(IntPtr windowHandle,
            int left, int right, int top, int bottom)
        {
            // Get the system DPI. 
            System.Drawing.Graphics g = System.Drawing.Graphics.FromHwnd(windowHandle);
            float desktopDpiX = g.DpiX;
            float desktopDpiY = g.DpiY;

            // Set the margins. 
            MARGINS margins = new MARGINS();
            margins.cxLeftWidth = Convert.ToInt32(left * (desktopDpiX / 96));
            margins.cxRightWidth = Convert.ToInt32(right * (desktopDpiX / 96));
            margins.cyTopHeight = Convert.ToInt32(top * (desktopDpiX / 96));
            margins.cyBottomHeight = Convert.ToInt32(right * (desktopDpiX / 96));

            return margins;
        } 
        private void ApplyGlass()
        {
            if (DwmApi.DwmIsCompositionEnabled())
            {
                var hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
                WindowInteropHelper windowInteropHelper = new WindowInteropHelper(this);
                IntPtr myHwnd = windowInteropHelper.Handle;
                HwndSource mainWindowSrc = System.Windows.Interop.HwndSource.FromHwnd(myHwnd);

                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                MARGINS margins = GetDpiAdjustedMargins(myHwnd, 0, 100, 0, 0);

                DwmExtendFrameIntoClientArea(myHwnd, ref margins);
            }
            else
            {
                this.Background = SystemColors.WindowFrameBrush;
                this.WindowStyle = System.Windows.WindowStyle.None;
            }
        }
        #endregion 

        #region Events
        private void StartMenu_SourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }
        private void btnAddActivity_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnOptions_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            taskbar.ExitApplication();
        }
        private void btnChangeWorkspace_Click(object sender, RoutedEventArgs e)
        {

        }
        private void workspaces_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {

        }
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            //this.Title = "Workspace: "+Settings.Default.AMWorkspace + "  Activity: " + taskbar.ActivityManager.SelectedActivity.Name;

        }
        private void btnLoadActivity_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnContext_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnSaveActivity_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnLogger_Click(object sender, RoutedEventArgs e)
        {
     
        }
        private void btnLogOff_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
          
        }
        #endregion

    }
}