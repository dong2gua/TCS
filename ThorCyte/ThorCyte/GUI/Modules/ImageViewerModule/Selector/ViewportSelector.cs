using System.Windows;
using System.Windows.Controls;
using System;
using System.Collections.Generic;
using ThorCyte.ImageViewerModule.View;
namespace ThorCyte.ImageViewerModule.Selector
{
  public  class ViewportSelector:DataTemplateSelector
    {
        public DataTemplate OneViewportDataTemplate { get; set; }
        public DataTemplate HorizontalViewportDataTemplate { get; set; }
        public DataTemplate VerticalViewportDataTemplate { get; set; }
        public DataTemplate AllViewportDataTemplate { get; set; }
        public override DataTemplate SelectTemplate(object item,    DependencyObject container)
        {
            if (item == null)
                return OneViewportDataTemplate;
            var type = item as ViewportDisplayType;
            if (type == null) return OneViewportDataTemplate;
            switch (type.Type)
            {
                case 1: return OneViewportDataTemplate;
                case 2: return HorizontalViewportDataTemplate;
                case 3: return VerticalViewportDataTemplate;
                case 4: return AllViewportDataTemplate;
                default: return OneViewportDataTemplate;
            }
        }

    }
    public class ViewportDisplayType
    {
        public int Type { get; set; }
        public List<ViewportView> Viewports { get; set; }
    }
}
