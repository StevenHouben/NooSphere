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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using ActivityUI.Xaml;
using NooSphere.Platform.Windows.Glass;
using Brushes = System.Windows.Media.Brushes;
using Color = System.Windows.Media.Color;
using Image = System.Windows.Controls.Image;
using SystemColors = System.Windows.SystemColors;

namespace ActivityUI
{
    public partial class StartMenu : Window
    {
        #region Members

        private readonly ActivityBar taskbar;

        #endregion

        #region Constructor

        public StartMenu(ActivityBar tBar)
        {
            InitializeComponent();

            ShowInTaskbar = false;

            SourceInitialized += StartMenu_SourceInitialized;
            taskbar = tBar;

            MinHeight = MaxHeight = Height;
            MinWidth = MaxWidth = Width;

            Top = taskbar.Height;
            Left = 4;
        }

        #endregion

        #region Window Ajustments

        /// <summary>
        /// Disabled move/box
        /// </summary>
        private const int WM_SYSCOMMAND = 0x0112;

        private const int SC_MOVE = 0xF010;

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

        public string Username
        {
            get { return lblName.Text; }
            set { lblName.Text = value; }
        }

        public void SetImage(Bitmap source)
        {
            imgMe.Source = WFormsImageToWPFImage(source).Source;
        }

        public Image WFormsImageToWPFImage(System.Drawing.Image Old_School_Image)
        {
            var ms = new MemoryStream();
            Old_School_Image.Save(ms, ImageFormat.Png);

            var bImg = new BitmapImage();
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(ms.ToArray());
            bImg.EndInit();
            var WPFImage = new Image();
            WPFImage.Source = bImg;
            return WPFImage;
            ;
        }

        #endregion

        #region Glass

        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        private WindowInteropHelper helper;

        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmEnableBlurBehindWindow(IntPtr hWnd, ref DWM_BLURBEHIND pBlurBehind);

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            //ApplyDarkGlass();
            ApplyGlass();
        }

        private void InitializeGlass()
        {
            helper = new WindowInteropHelper(this);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("DwmApi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        private static extern IntPtr DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private void ApplyDarkGlass()
        {
            int val = 1;
            IntPtr result = DwmSetWindowAttribute(new WindowInteropHelper(this).Handle, 3, ref val, sizeof (int));
        }

        private MARGINS GetDpiAdjustedMargins(IntPtr windowHandle,
                                              int left, int right, int top, int bottom)
        {
            var margins = new MARGINS();
            margins.cxLeftWidth = Convert.ToInt32(left);
            margins.cxRightWidth = Convert.ToInt32(right);
            margins.cyTopHeight = Convert.ToInt32(top);
            margins.cyBottomHeight = Convert.ToInt32(bottom);

            return margins;
        }

        private void ApplyGlass()
        {
            Background = Brushes.Transparent;
            if (DwmApi.DwmIsCompositionEnabled())
            {
                IntPtr hwnd = new WindowInteropHelper(this).Handle;
                SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
                var windowInteropHelper = new WindowInteropHelper(this);
                IntPtr myHwnd = windowInteropHelper.Handle;
                HwndSource mainWindowSrc = HwndSource.FromHwnd(myHwnd);

                mainWindowSrc.CompositionTarget.BackgroundColor = Color.FromArgb(0, 0, 0, 0);

                MARGINS margins = GetDpiAdjustedMargins(myHwnd, 0, 145, 0, 0);

                DwmExtendFrameIntoClientArea(myHwnd, ref margins);
            }
            else
            {
                Background = SystemColors.WindowFrameBrush;
                WindowStyle = WindowStyle.None;
            }
        }

        #region Nested type: DWMWINDOWATTRIBUTE

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

        #endregion

        #region Nested type: DWM_BLURBEHIND

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public readonly DwmBlurBehindFlags dwFlags;
            public readonly bool fEnable;
            public readonly IntPtr hRgnBlur;
            public readonly bool fTransitionOnMaximized;
        }

        #endregion

        #region Nested type: DwmBlurBehindFlags

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

        #endregion

        #region Nested type: MARGINS

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int cxLeftWidth; // width of left border that retains its size
            public int cxRightWidth; // width of right border that retains its size
            public int cyTopHeight; // height of top border that retains its size
            public int cyBottomHeight; // height of bottom border that retains its size
        };

        #endregion

        #endregion

        #region Events

        private void StartMenu_SourceInitialized(object sender, EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        private void btnAddActivity_Click(object sender, RoutedEventArgs e)
        {
            taskbar.AddEmptyActivity();
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

        private void workspaces_MouseLeave(object sender, MouseEventArgs e)
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