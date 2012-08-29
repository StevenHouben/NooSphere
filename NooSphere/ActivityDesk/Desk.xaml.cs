/****************************************************************************
 (c) 2012 Steven Houben(shou@itu.dk) and Søren Nielsen(snielsen@itu.dk)

 Pervasive Interaction Technology Laboratory (pIT lab)
 IT University of Copenhagen

 This library is free software; you can redistribute it and/or 
 modify it under the terms of the GNU GENERAL PUBLIC LICENSE V3 or later, 
 as published by the Free Software Foundation. Check 
 http://www.gnu.org/licenses/gpl.html for details.
****************************************************************************/

using System;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Media;
using Microsoft.Surface.Presentation.Controls.TouchVisualizations;
using ActivityDesk.Windowing;

using System.Threading;
using ActivityDesk.Visualizer.Definitions;
using System.Windows.Threading;
using System.Collections.Generic;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.Core.ActivityModel;
using NooSphere.ActivitySystem.Host;
using NooSphere.ActivitySystem.Discovery;
using NooSphere.Core.Devices;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Runtime.InteropServices;
using NooSphere.ActivitySystem.Base.Client;
using System.Windows.Controls;
using ActivityDesk.Visualizer.Visualization;
using System.Threading.Tasks;

namespace ActivityDesk
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Desk : SurfaceWindow
    {
        #region Members

        private ActivityClient _pairedManager;
        private string _pairedTag;
        private GenericHost _host;
        private DiscoveryManager _disc;
        private readonly User _user;
        private readonly Device _device;
        private DeskState _deskState;
        private Activity _currentActivity;
        private readonly List<string> _lockedTags = new List<string>();
        private readonly List<string> _connectedDeviceTags = new List<string>();
        private Dictionary<Guid, SurfaceButton> _proxies = new Dictionary<Guid, SurfaceButton>();

        #endregion

        #region Constructor
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Desk()
        {
            //this._user = user;
            //Initializes design-time components
            InitializeComponent();

            //Disable default touch visualizations
            TouchVisualizer.SetShowsVisualizations(this, false);

            //Initializes tag definitions
            InitializeTags();

            SetDeskState(DeskState.Ready);

            _device = new Device()
            {
                DevicePortability = DevicePortability.Stationary,
                DeviceRole = DeviceRole.Mediator,
                DeviceType = DeviceType.Tabletop,
                Name = "Surface"
            };

        }

        public BitmapSource ToBitmapSource(System.Drawing.Bitmap source)
        {
            BitmapSource bitSrc = null;

            var hBitmap = source.GetHbitmap();

            try
            {
                bitSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());
            }
            catch (Win32Exception)
            {
                bitSrc = null;
            }
            finally
            {
                NativeMethods.DeleteObject(hBitmap);
            }

            return bitSrc;
        }
        internal static class NativeMethods
        {
            [DllImport("gdi32.dll")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool DeleteObject(IntPtr hObject);
        }

        private void SetDeskState(ActivityDesk.DeskState deskState)
        {
            this._deskState = deskState;

            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                switch (deskState)
                {
                    case ActivityDesk.DeskState.Active:
                        this.Background = (ImageBrush)this.Resources["green"];
                        break;
                    case ActivityDesk.DeskState.Locked:
                        this.Background = (ImageBrush)this.Resources["red"];
                        break;
                    case ActivityDesk.DeskState.Occupied:
                        this.Background = (ImageBrush)this.Resources["yellow"];
                        break;
                    case ActivityDesk.DeskState.Ready:
                        this.Background = (ImageBrush)this.Resources["blue"];
                        break;
                }
            }));
        }
        #endregion

        #region Initializers
        /// <summary>
        /// Initializes the User Interface
        /// </summary>
        private void InitializeUI()
        {
            SetDeskState(ActivityDesk.DeskState.Occupied);
        }

        /// <summary>
        /// Initializes the tag defintions
        /// </summary>
        private void InitializeTags()
        {
            TagVisualizationDefinition definition = new SmartPhoneDefinition();
            definition.Source = new Uri("Visualizer/Visualizations/SmartPhone.xaml", UriKind.Relative);
            definition.TagRemovedBehavior = TagRemovedBehavior.Disappear;
            definition.LostTagTimeout = 1000;

            TagVisualizationDefinition definition2 = new TabletDefinition();
            definition2.Source = new Uri("Visualizer/Visualizations/VisualizationTablet.xaml", UriKind.Relative);
            definition2.LostTagTimeout = 1000;

            Visualizer.Definitions.Add(definition);
            Visualizer.Definitions.Add(definition2);
        }
        #endregion

        #region NooSphere

        private void StartHost()
        {
            var t = new Thread(() =>
            {
                _host = new GenericHost(7891);
                _host.HostLaunched += new HostLaunchedHandler(host_HostLaunched);
                _host.Open(new ActivityManager(new User(), "c:/files/"), typeof(IActivityManager), "desk");

            });
            t.Start();
        }
        private void StartClient(string addr,string tag)
        {
            if (_pairedManager == null)
            {
                try
                {
                    _pairedTag = tag;
                    _pairedManager = new ActivityClient(@"c:/abcdesk/", _device) { CurrentUser = new User() };
                    _pairedManager.ActivityAdded += ClientActivityAdded;
                    _pairedManager.ActivityRemoved += ClientActivityRemoved;
                    _pairedManager.FileAdded += new FileAddedHandler(_client_FileAdded);
                    _pairedManager.ServiceIsDown += new ServiceDownHandler(_client_ServiceIsDown);
                    _pairedManager.ActivitySwitched += new ActivitySwitchedHandler(_client_ActivitySwitched);
                    _pairedManager.Open(addr);
                    InitializeUI();
                }
                catch (Exception ex)
                {

                    MessageBox.Show(ex.ToString());
                }
            }
            else if (tag != _pairedTag)
            {
                StartRoleChangeProcedure(addr);
                return;
            }

        }

        private void StartRoleChangeProcedure(string addr)
        {
            try
            {
                var actCli = new ActivityClient(@"c:/abcdesk/", _device) { CurrentUser = new User() };
                actCli.ConnectionEstablished += ActCliConnectionEstablished;
                actCli.Open(addr);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }

        private void ActCliConnectionEstablished(object sender, EventArgs e)
        {
            var msg= new Message
                         {Type = MessageType.Connect, Content = _pairedManager.ServiceAddress, From = "Mediator"};
            ((ActivityClient)sender).SendMessage(msg);
        }
        private void RunDiscovery()
        {
            Task.Factory.StartNew(() =>
            {
                _disc = new DiscoveryManager();
                _disc.DiscoveryAddressAdded += DiscDiscoveryAddressAdded;
                _disc.DiscoveryFinished += DiscDiscoveryFinished;
                _disc.Find(DiscoveryType.WSDiscovery);
            });
        }
        private void DiscDiscoveryFinished(object o, DiscoveryEventArgs e)
        {
            if (_disc.ActivityServices.Count == 0)
                if (Visualizer.ActiveVisualizations.Count == 0)
                    SetDeskState(DeskState.Ready);
                else
                    SetDeskState(DeskState.Locked);
        }
        private void DiscDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            var q = new List<TagVisualization>(Visualizer.ActiveVisualizations);

            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                foreach (var tv in q)
                {
                    if (e.ServiceInfo.Code == Convert.ToString(tv.VisualizedTag.Value))
                    {
                        StartClient(e.ServiceInfo.Address, e.ServiceInfo.Code);
                    }
                }
            }));
        }
        private void host_HostLaunched(object sender, EventArgs e)
        {
            //StartClient(_host.Address,);
        }
        private void _client_ActivitySwitched(object sender, ActivityEventArgs e)
        {
            _currentActivity = e.Activity;
            view.Items.Clear();
            if (e.Activity.Resources.Count != 0)
                foreach (Resource res in e.Activity.Resources)
                    VisualizeResouce(res, _pairedManager.LocalPath + res.RelativePath);
        }
        private void _client_ServiceIsDown(object sender, EventArgs e)
        {
            SetDeskState(ActivityDesk.DeskState.Locked);

            if (_pairedManager != null)
            {
                _pairedManager.Close();
                _pairedManager = null;
            }
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                view.Items.Clear();
            }));
        }
        private void _client_FileAdded(object sender, FileEventArgs e)
        {
            if(_currentActivity != null)
                if(e.Resource.ActivityId == _currentActivity.Id)
                    VisualizeResouce(e.Resource,e.LocalPath);
        }
        private void ClientActivityAdded(object sender, ActivityEventArgs e)
        {
            //AddActivityUI(e.Activity);
        }
        private void client_DeviceAdded(object sender, DeviceEventArgs e)
        {
           
        }
        private void client_MessageReceived(object sender, ComEventArgs e)
        {
            MessageBox.Show(e.Message.Content);

        }
        private void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            //RemoveActivityUI(e.Id);
        }
        #endregion

        #region UI
        private void VisualizeResouce(Resource res, string path)
        {
            try
            {
                var i = new Image();
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource = new Uri(path, UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                i.Source = src;
                i.Stretch = Stretch.Uniform;

                view.Items.Add(i);
            }
            catch { }
        }
        private void AddResourceWindow()
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                view.Items.Add(new TableWindow());
            }));
        }
        private void AddActivityUI(Activity activity)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                SurfaceButton b = new SurfaceButton();
                b.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
                b.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
                b.Background = System.Windows.Media.Brushes.Gray;
                b.Width = 300;
                b.Tag = activity.Id;
                b.Height = Double.NaN;
                b.Content = activity.Name;

                view.Items.Add(b);
            }));
        }
        private void RemoveActivityUI(Guid guid)
        {
            this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                for (int i = 0; i < view.Items.Count; i++)
                {
                    if (view.Items[i] is SurfaceButton)
                        if (((Guid)((SurfaceButton)view.Items[i]).Tag) == guid)
                            view.Items.RemoveAt(i);
                }
            }));
        }
        #endregion

        #region Events
        private void Visualizer_VisualizationAdded(object sender, TagVisualizerEventArgs e)
        {
            if (!_lockedTags.Contains(e.TagVisualization.VisualizedTag.Value.ToString()))
            {
                if (Visualizer.ActiveVisualizations.Count >0 && _pairedManager ==null)
                {
                    SetDeskState(ActivityDesk.DeskState.Active);
                }
            }
            ((BaseVisualization)e.TagVisualization).Locked += new LockedEventHandler(Desk_Locked);
            RunDiscovery();
        }
        private void Desk_Locked(object sender, LockedEventArgs e)
        {
            if (_lockedTags.Contains(e.VisualizedTag))
            {
                _lockedTags.Remove(e.VisualizedTag);
                Log.Out("ActivityDesk", String.Format("{0} unlocked", e.VisualizedTag));
            }
            else
            {
                _lockedTags.Add(e.VisualizedTag);
                Log.Out("ActivityDesk", String.Format("{0} locked", e.VisualizedTag));
            }
        }
        private void Visualizer_VisualizationRemoved(object sender, TagVisualizerEventArgs e)
        {
            if (!_lockedTags.Contains(e.TagVisualization.VisualizedTag.Value.ToString()))
            {
                Thread.Sleep(3000);
                if (Visualizer.ActiveVisualizations.Count == 0)
                {
                    SetDeskState(ActivityDesk.DeskState.Ready);
                }
                if (_pairedManager != null)
                {
                    _pairedManager.Close();
                    _pairedManager = null;
                }


                this.Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
                {
                    view.Items.Clear();
                }));
            }
        }
        private void Visualizer_VisualizationMoved(object sender, TagVisualizerEventArgs e)
        {

        }
        #endregion
    }
}