using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Surface.Presentation.Controls;

namespace ActivityDesk
{
    public class DocumentViewTemplateSelector : DataTemplateSelector
    {
      public DataTemplate FullSize { get; set; }
      public DataTemplate Docked { get; set; }

      public override DataTemplate SelectTemplate(object item, DependencyObject container)
      {
          if (((DeviceTumbnail)item).Center.X < 100 || ((DeviceTumbnail)item).Center.X < 1900)
              return Docked;
          else
              return FullSize;
      }
    }
}
