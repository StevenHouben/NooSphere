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
        private readonly int _rightDockX = 75;
        private readonly int _leftDockX = 1845;
        private int _leftDockTreshhold = 100;
        private int _rightDockTreshhold = 1820;

        public DocumentContainer()
        {
            InitializeComponent();

            //register dockstate dependency property
            var metadata = new FrameworkPropertyMetadata(DockStates.Free);
            DockState = DependencyProperty.RegisterAttached("DockState",
                                                                typeof(DockStates),
                                                                typeof(DocumentContainer), metadata);
        }

        public void Clear()
        {
            this.view.Items.Clear();
        }

        public void Add(ScatterViewItem element)
        {
            DocumentContainer.SetDockState(element, DockStates.Free);
            element.AddHandler(UIElement.ManipulationCompletedEvent, new EventHandler<ManipulationCompletedEventArgs>(element_ManipulationDelta), true);
            element.PreviewTouchMove += new EventHandler<TouchEventArgs>(element_PreviewTouchMove);
            ((ScatterViewItem)element).Template = (ControlTemplate)element.FindResource("Floating");
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
            else
            {
                item.Template = (ControlTemplate)item.FindResource("Floating");
                DocumentContainer.SetDockState(item, DockStates.Free);
            }
            UpdateDock(item);
        }
        private void HandleDocking(ScatterViewItem item)
        {
            if (item.Center.X < _leftDockTreshhold)
            {
                item.Template = (ControlTemplate)item.FindResource("Docked");
                DocumentContainer.SetDockState(item, DockStates.Left);
            }
            else if (item.Center.X > _rightDockTreshhold)
            {
                item.Template = (ControlTemplate)item.FindResource("Docked");
                DocumentContainer.SetDockState(item, DockStates.Right);
            }
            else
            {
                item.Template = (ControlTemplate)item.FindResource("Floating");
                DocumentContainer.SetDockState(item, DockStates.Free);
            }
            UpdateDock(item);
        }
        private void UpdateDock(ScatterViewItem item)
        {
            var state = DocumentContainer.GetDockState(item);
            if (state == DockStates.Free)
            {
                item.CanMove = true;
                item.CanRotate = true;
            }
            else
            {
                if (state == DockStates.Left)
                    item.Center = new Point(_rightDockX, item.Center.Y);
                else
                    item.Center = new Point(_leftDockTreshhold, item.Center.Y);
                item.Orientation = 0;
                item.CanMove = true;
                item.CanRotate = false;
                Console.WriteLine(item.Center.X.ToString() + "  -> " + item.Width.ToString());
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
    }
    public enum DockStates
    {
        Left,
        Right,
        Free
    }
}
