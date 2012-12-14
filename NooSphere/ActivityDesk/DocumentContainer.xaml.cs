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
using Microsoft.Surface.Presentation.Controls;
using System.Collections.ObjectModel;
using NooSphere.Core.Devices;

namespace ActivityDesk
{
    /// <summary>
    /// Interaction logic for DocumentView.xaml
    /// </summary>
    public partial class DocumentContainer : UserControl
    {
        #region Image dependency property


        public static DependencyProperty DockState;

        public static DockStates GetDockState(DependencyObject obj)
        {
          return (DockStates) obj.GetValue(DockState);
        }
        public static void SetDockState(DependencyObject obj, DockStates value)
        {
          obj.SetValue(DockState, value);
        }

        #endregion

        private readonly int _dockSize = 150;
        private readonly int _rightDockX = 1845;
        private readonly int _leftDockX = 75;
        private readonly int _upperDockY = 75;
        private readonly int _upperDockThreshold = 100;
        private readonly int _leftDockTreshhold = 100;
        private readonly int _rightDockTreshhold = 1820;

        public Collection<Note> Notes = new Collection<Note>();
        public Collection<ResourceViewer> ResourceViewers = new Collection<ResourceViewer>();
        public Dictionary<string, DeviceTumbnail> Devices = new Dictionary<string, DeviceTumbnail>();

        public DocumentContainer()
        {
            InitializeComponent();

            //register dockstate dependency property
            var metadata = new FrameworkPropertyMetadata(DockStates.Floating);
            DockState = DependencyProperty.RegisterAttached("DockState",
                                                                typeof(DockStates),
                                                                typeof(DocumentContainer), metadata);
        }
        public void Clear()
        {
            this.view.Items.Clear();
        }
        public void AddNote()
        {
            var ink = new Note();
            ink.Center = new Point(450, 450);
            ink.Close += new EventHandler(ink_Close);
            Notes.Add(ink);
            this.Add(ink);
        }
        public void AddResource(Image img,string text)
        {
            var res = new ResourceViewer(img,text);
            ResourceViewers.Add(res);
            this.Add(res);
        }
        public void AddDevice(Device device,Point position)
        {
            var dev = new DeviceTumbnail();
            dev.Name = device.Name;
            dev.Center = position;
            Devices.Add(device.TagValue.ToString(),dev);
            this.Add(dev);
        }
        public void UpdateDevice(Device device)
        {
            Devices[device.TagValue.ToString()].Name = device.Name;
        }
        private void ink_Close(object sender, EventArgs e)
        {

        }
        private void Add(ScatterViewItem element)
        {
            DocumentContainer.SetDockState(element, DockStates.Floating);
            element.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(element_ManipulationDelta), true);
            element.PreviewTouchMove += new EventHandler<TouchEventArgs>(element_PreviewTouchMove);
            ((ScatterViewItem)element).Template = (ControlTemplate)element.FindResource("Floating");
            element.Orientation = 0;
            element.CanRotate = false;
            this.view.Items.Add(element);
        }
        private void element_PreviewTouchMove(object sender, TouchEventArgs e)
        {
            HandleDockingFromTouch((ScatterViewItem)sender,e.GetTouchPoint(view).Position);
        }
        private void HandleDockingFromTouch(ScatterViewItem item,Point p)
        {
            if (p.X < _leftDockTreshhold)
            {
                item.Template = (ControlTemplate)item.FindResource("Docked");
                DocumentContainer.SetDockState(item, DockStates.Left);
            }
            else if (p.X > _rightDockTreshhold)
            {
                item.Template = (ControlTemplate)item.FindResource("Docked");
                DocumentContainer.SetDockState(item, DockStates.Right);
            }
            else if (p.Y < _upperDockThreshold)
            {
                item.Template = (ControlTemplate)item.FindResource("Docked");
                DocumentContainer.SetDockState(item, DockStates.Top);
            }
            else
            {
                item.Template = (ControlTemplate)item.FindResource("Floating");
                DocumentContainer.SetDockState(item, DockStates.Floating);
            }
            UpdateDock(item);
        }
        private void HandleDocking(ScatterViewItem item)
        {
            HandleDockingFromTouch(item,item.Center);
        }
        private void UpdateDock(ScatterViewItem item)
        {
            var state = DocumentContainer.GetDockState(item);
            if (state == DockStates.Floating)
                item.CanMove = true;
            else
            {
                if (state == DockStates.Left)
                    item.Center = new Point(_leftDockX, item.Center.Y);
                else if(state == DockStates.Right)
                    item.Center = new Point(_rightDockX, item.Center.Y);
                else
                    item.Center = new Point(item.Center.X, _upperDockY);
                item.Orientation = 0;
                item.CanMove = true;
                //item.CanRotate = false;
            }
        }
        void element_ManipulationDelta(object sender, ManipulationCompletedEventArgs e)
        {
            HandleDocking((ScatterViewItem)sender);
        }

        public void Remove(object element)
        {
            this.view.Items.Remove(element);
        }

        public void RemoveDevice(string p)
        {
            if(Devices.ContainsKey(p))
            {
                view.Items.Remove(Devices[p]);
                Devices.Remove(p);
            }
        }
    }
    public enum DockStates
    {
        Left,
        Right,
        Floating,
        Top
    }
}
