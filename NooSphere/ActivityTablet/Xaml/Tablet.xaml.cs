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
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Controls;
using NooSphere.ActivitySystem.Base;
using NooSphere.ActivitySystem.Base.Client;
using NooSphere.ActivitySystem.Base.Service;
using NooSphere.ActivitySystem.Helpers;
using NooSphere.ActivitySystem.Host;
using NooSphere.Core.ActivityModel;
using Newtonsoft.Json;
using ActivityTablet.Properties;
using NooSphere.Core.Devices;
using NooSphere.ActivitySystem.Discovery;

namespace ActivityTablet.Xaml
{
    public partial class Tablet
    {
        #region Private Members
        private ActivityClient _client;
        private GenericHost _host;
        private User _user;
        private Device _device;
        private readonly Dictionary<Guid, Proxy> _proxies = new Dictionary<Guid, Proxy>();
        private Activity _currentActivity;

        #endregion

        #region Constructor
        public Tablet()
        {
            //Initializes design-time components
            InitializeComponent();

            resourceViewer.Visibility = Visibility.Hidden;
            inputView.Visibility = Visibility.Hidden;
            menu.Visibility = Visibility.Hidden;
            LoadSettings();
        }
        #endregion

        #region Private Methods
        private void LogIn()
        {
            try
            {
                string baseUrl = Settings.Default.ENVIRONMENT_BASE_URL;
                string result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
                User u = JsonConvert.DeserializeObject<User>(result);
                if (u != null)
                    this._user = u;
                else
                    CreateUser(baseUrl);

                this._device = new Device();
                this._device.Name = txtDevicename.Text;

                if (chkClient.IsChecked == true)
                    FindClient();
                else
                    StartActivityManager();
                SaveSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void FindClient()
        {
            RunDiscovery();
        }
        private void RunDiscovery()
        {
            try
            {
                Task.Factory.StartNew(delegate {  
                    var disc = new DiscoveryManager();
                    disc.DiscoveryAddressAdded += new DiscoveryManager.DiscoveryAddressAddedHandler(DiscDiscoveryAddressAdded);
                    disc.Find();
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());

            }

        }
        private void StartActivityManager()
        {
            Task.Factory.StartNew(
                delegate {
                _host = new GenericHost();
                _host.HostLaunched += HostHostLaunched;
                _host.Open(new ActivityManager(_user, "c:/files/"), typeof(IActivityManager), "Tablet manager");
                _host.StartBroadcast(DiscoveryType.WSDiscovery, "Tablet", "205");
            });
        }
        private void BuildUI()
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                //Hide login
                cvLogin.Visibility = Visibility.Hidden;

                //Show menu
                menu.Visibility = Visibility.Visible;
                
                //Show resource mode by default
                resourceViewer.Visibility = Visibility.Visible;
            }));
        }
        private void AddActivityUI(Activity ac)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                var srfcBtn = new SurfaceButton {Width = activityScroller.Width, Tag = ac.Id};
                var p = new Proxy {Activity = ac, Ui = srfcBtn};
                srfcBtn.Content = ac.Name;
                srfcBtn.Click += SrfcBtnClick;
                activityStack.Children.Add(srfcBtn);

                var mtrBtn = CopyButton(srfcBtn);
                mtrBtn.Width = mtrBtn.Height = 200;
                mtrBtn.Click += SrfcBtnClick;
                activityMatrix.Children.Add(mtrBtn);
                _proxies.Add(p.Activity.Id, p);
            }));
        }
        private SurfaceButton CopyButton(SurfaceButton btn)
        {
            return new SurfaceButton
                       {
                           Content = btn.Content, 
                           Tag=btn.Tag, 
                           VerticalContentAlignment = VerticalAlignment.Center,
                           HorizontalContentAlignment = HorizontalAlignment.Center
                       };
        }
        private void SrfcBtnClick(object sender, RoutedEventArgs e)
        {
            _client.SwitchActivity(_proxies[(Guid)((SurfaceButton)sender).Tag].Activity);
        }
        private void RemoveActivityUI(Guid id)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                for (int i = 0; i < activityStack.Children.Count;i++ )
                    if((Guid)((SurfaceButton)activityStack.Children[i]).Tag ==id)
                    {
                        activityStack.Children.RemoveAt(i);
                        activityMatrix.Children.RemoveAt(i);
                    }
                _proxies.Remove(id);
            }));
        }
        private void LoadSettings()
        {
            txtUsername.Text = Settings.Default.USER_NAME;
            txtEmail.Text = Settings.Default.USER_EMAIL;
            txtDevicename.Text = Settings.Default.USER_DEVICENAME;
        }
        private void SaveSettings()
        {
            Settings.Default.USER_NAME = txtUsername.Text;
            Settings.Default.USER_EMAIL = txtEmail.Text;
            Settings.Default.USER_DEVICENAME = txtDevicename.Text;
            Settings.Default.Save();
        }
        private void CreateUser(string baseUrl)
        {
            var user = new User {Email = txtEmail.Text, Name = txtUsername.Text};
            var added = Rest.Post(baseUrl + "Users", user);
            if (JsonConvert.DeserializeObject<bool>(added))
            {
                var result = Rest.Get(baseUrl + "Users?email=" + txtEmail.Text);
                var u = JsonConvert.DeserializeObject<User>(result);
                this._user = u;
            }
        }
        private void StartClient(string addr)
        {
            if (_client != null)
                return;
            try
            {
                Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
                {
                    activityStack.Children.Clear();
                    activityMatrix.Children.Clear();
                }));
                _client = new ActivityClient(@"c:/abc/", _device) { CurrentUser = new User() };
                _client.MessageReceived += ClientMessageReceived;
                _client.ActivityAdded += ClientActivityAdded;
                _client.ActivityRemoved += ClientActivityRemoved;
                _client.ConnectionEstablished += ClientConnectionEstablished;
                _client.FileAdded += ClientFileAdded;
                _client.ActivitySwitched += ClientActivitySwitched;
                
                _client.Open(addr);
            }
            catch (Exception ex)
            {

                MessageBox.Show(ex.ToString());
            }
        }
        private void ShowResource(object sender)
        {
            try
            {
                ContentHolder.Strokes.Clear();

                var src = ((Image)sender).Source;
                ContentHolder.Height = src.Height;
                ContentHolder.Background = new ImageBrush(src);
                ContentHolder.Tag = ((Image) sender).Tag;
                //{
                //    Source = ((Image)sender).Source,
                //    MaxHeight = ContentScroller.Height,
                //    MaxWidth = ContentScroller.Width
                //};
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        private void PopulateResource(Activity activity)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                ContentHolder.Strokes.Clear();
                resourceDock.Children.Clear();
                ContentHolder.Background = null;
                foreach (
                    var resource in
                        activity.Resources)
                {
                    AddResource(resource,
                                _client.
                                    LocalPath +
                                resource.
                                    RelativePath);
                }
            }));
        }
        private void AddResource(Resource resource,string path)
        {
            Dispatcher.Invoke(DispatcherPriority.Background, new System.Action(() =>
            {
                if (Path.GetExtension(path) == ".pdf")
                    _client.AddResource(new FileInfo(PDFConverter.Convert(path)),resource.ActivityId);
                else
                    TryToAddImage(resource, path);
            }));
        }

        private void TryToAddImage(Resource resource, string path)
        {
            try
            {
                var i = new Image {Tag = resource};
                i.Width = i.Height = 100;
                var src = new BitmapImage();
                src.BeginInit();
                src.UriSource =
                    new Uri(path,
                            UriKind.
                                Relative);
                src.CacheOption =
                    BitmapCacheOption.
                        OnLoad;
                src.EndInit();
                i.Source = src;
                i.Stretch = Stretch.Uniform;
                i.MouseDown += IMouseDown;
                i.TouchDown += ITouchDown;

                resourceDock.Children.Add(i);
            }
            catch (Exception)
            {
                //not an image -> do better implementation here
            }
        }

        private void HandleMessage(Message message)
        {
            if(message.Type==MessageType.Connect)
            {
                _client = null;
                StartClient(message.Content);
            }
        }
        #endregion

        #region Events Handlers
        private void ITouchDown(object sender, TouchEventArgs e)
        {
            ShowResource(sender);
        }
        private void ClientActivityAdded(object obj, ActivityEventArgs e)
        {
            AddActivityUI(e.Activity);
            _currentActivity = e.Activity;
        }
        private void ClientActivitySwitched(object sender, ActivityEventArgs e)
        {
            _currentActivity = e.Activity;
            PopulateResource(e.Activity);
        }
        private void ClientFileAdded(object sender, FileEventArgs e)
        {
            if (e.Resource.ActivityId != _currentActivity.Id)
                return;
            AddResource(e.Resource, e.LocalPath);
        }
        private void ClientActivityRemoved(object sender, ActivityRemovedEventArgs e)
        {
            RemoveActivityUI(e.Id);
        }
        private void ClientMessageReceived(object sender, ComEventArgs e)
        {
            HandleMessage(e.Message);
        }
        private void ClientConnectionEstablished(object sender, EventArgs e)
        {
            BuildUI();
        }
        private void BtnAddClick(object sender, RoutedEventArgs e)
        {
            _client.AddActivity(GetInitializedActivity());
        }
        private void IMouseDown(object sender, MouseButtonEventArgs e)
        {
            ShowResource(sender);
        }
        private void BtnQuitClick(object sender, RoutedEventArgs e)
        {
            ExitApplication();
        }
        private void DiscDiscoveryAddressAdded(object o, DiscoveryAddressAddedEventArgs e)
        {
            if(e.ServiceInfo.Code != "139")
                StartClient(e.ServiceInfo.Address);
        }
        private void BtnGoClick(object sender, RoutedEventArgs e)
        {
            LogIn();
        }
        private void HostHostLaunched(object sender, EventArgs e)
        {
            StartClient(_host.Address);
        }
        private void ExitApplication()
        {
            if (_client != null)
                _client.Close();
            Environment.Exit(0);
        }
        #endregion

        #region Helpers
        public Activity GetInitializedActivity()
        {
            var ac = new Activity
            {
                Name = "nameless",
                Description = "This is the description of the test activity - " + DateTime.Now
            };
            ac.Uri = "http://tempori.org/" + ac.Id;
            ac.Participants.Add(new User() { Email = " 	snielsen@itu.dk" });
            ac.Meta.Data = "added meta data";
            ac.Owner = _user;
            return ac;
        }
        #endregion

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            var path = _client.LocalPath + ((Resource) ContentHolder.Tag).RelativePath;
            var filename = Path.GetFileNameWithoutExtension(path);
            var ext = Path.GetExtension(path);
            if (!Directory.Exists("c:/temp/"))
                Directory.CreateDirectory("c:/temp/");
            var newFile = new Uri("c:/temp/" + filename + "_edit"+Guid.NewGuid().ToString() + ext);
            SaveToFile(newFile, ContentHolder);
            _client.AddResource(new FileInfo(newFile.AbsolutePath), _currentActivity.Id);

            //PopulateResource(_currentActivity);
        }

        private void SaveToFile(Uri path, InkCanvas surface)
        {
            //get the dimensions of the ink control
            var margin = (int)surface.Margin.Left;
            var width = (int)surface.ActualWidth - margin;
            var height = (int)surface.ActualHeight - margin;
            //render ink to bitmap
            var rtb = new RenderTargetBitmap(width, height, 96d, 96d, PixelFormats.Default);
            rtb.Render(surface);
            //save the ink to a memory stream
            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(rtb));
            using (var ms = new FileStream(path.LocalPath, FileMode.Create))
            {
                encoder.Save(ms);
            }
        }
        public void ExportToPng(Uri path, InkCanvas surface)
        {
            if (path == null) return;

            // Save current canvas transform
            Transform transform = surface.LayoutTransform;
            // reset current transform (in case it is scaled or rotated)
            surface.LayoutTransform = null;

            // Get the size of canvas
            Size size = new Size(surface.Width, surface.Height);
            // Measure and arrange the surface
            // VERY IMPORTANT
            surface.Measure(size);
            surface.Arrange(new Rect(size));

            // Create a render bitmap and push the surface to it
            RenderTargetBitmap renderBitmap =
              new RenderTargetBitmap(
                (int)size.Width,
                (int)size.Height,
                96d,
                96d,
                PixelFormats.Pbgra32);
            renderBitmap.Render(surface);

            // Create a file stream for saving image
            using (FileStream outStream = new FileStream(path.LocalPath, FileMode.Create))
            {
                surface.Strokes.Save(outStream);
                // Use png encoder for our data
                PngBitmapEncoder encoder = new PngBitmapEncoder();
                // push the rendered bitmap to it
                encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                // save the data to the stream
                encoder.Save(outStream);
            }

            // Restore previously saved layout
            surface.LayoutTransform = transform;
        }

        private void btnMode_Click(object sender, RoutedEventArgs e)
        {
            if (_displayMode == DisplayMode.ResourceViewer)
                _displayMode = DisplayMode.Controller;
            else
                _displayMode = DisplayMode.ResourceViewer;
            switch (_displayMode)
            {
                case DisplayMode.ResourceViewer:
                    resourceViewer.Visibility = Visibility.Visible;
                    inputView.Visibility = Visibility.Hidden;
                    controllerView.Visibility =Visibility.Hidden;
                    break;
                case DisplayMode.Controller:
                    resourceViewer.Visibility = Visibility.Hidden;
                    inputView.Visibility = Visibility.Hidden;
                    controllerView.Visibility = Visibility.Visible;
                    break;
            }
        }

        private DisplayMode _displayMode;
    }

    public enum DisplayMode
    {
        ResourceViewer,
        Controller,
        input
    }
}