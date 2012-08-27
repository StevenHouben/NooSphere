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
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using System.Windows.Media.Animation;
using ActivityDesk.Helper;

namespace ActivityDesk.Windowing
{
	public partial class TableWindow : UserControl
	{
		#region Attached Property InitialSizeRequest
		public static Size GetInitialSizeRequest(DependencyObject obj)
		{
			return (Size)obj.GetValue(InitialSizeRequestProperty);
		}

		public static void SetInitialSizeRequest(DependencyObject obj, Size value)
		{
			obj.SetValue(InitialSizeRequestProperty, value);
		}

		// Using a DependencyProperty as the backing store for InitialSizeRequest.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty InitialSizeRequestProperty =
			DependencyProperty.RegisterAttached("InitialSizeRequest", typeof(Size), typeof(TableWindow), new UIPropertyMetadata(Size.Empty));

		#endregion

		public TableWindow()
		{
			InitializeComponent();
			if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				c_contentHolder.Loaded += new RoutedEventHandler(c_contentHolder_Loaded);
		}

		public TableWindow(object content)
		{
			InitializeComponent();
			if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				c_contentHolder.Loaded += new RoutedEventHandler(c_contentHolder_Loaded);
		}

		private static Size DefaultPopupSize = new Size(300, 200);
		/// <summary>
		/// Gets the size that the parent container should have to fully accomodate the PopupWindow and its child content
		/// based on the child's InitialSizeRequest.
		/// </summary>
		/// <returns>The size, which should be set to the parent container</returns>
		private Size CalculateScatterViewItemSize()
		{
			var presenter = GuiHelpers.GetChildObject<ContentPresenter>(c_contentHolder);
			if (presenter == null)
				return DefaultPopupSize;
			// It seems it's safe to assume the ContentPresenter will always only have one child and that child is the visual representation
			// of the content of c_contentHolder.
			var child = VisualTreeHelper.GetChild(presenter, 0);
			if (child == null)
				return DefaultPopupSize;
			var requestedSize = TableWindow.GetInitialSizeRequest(child);
			if (!requestedSize.IsEmpty
				&& requestedSize.Width != 0
				&& requestedSize.Height != 0)
			{
				var borderHeight = this.ActualHeight - c_contentHolder.ActualHeight;
				var borderWidth = this.ActualWidth - c_contentHolder.ActualWidth;
				return new Size(requestedSize.Width + borderWidth, requestedSize.Height + borderHeight);
			}
			else
				return DefaultPopupSize;
		}

		void c_contentHolder_Loaded(object sender, RoutedEventArgs e)
		{
			var newSize = CalculateScatterViewItemSize();
		}
		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			var sv = GuiHelpers.GetParentObject<ScatterView>(this);
			if (sv != null)
				sv.Items.Remove(this);
		}
	}
}
