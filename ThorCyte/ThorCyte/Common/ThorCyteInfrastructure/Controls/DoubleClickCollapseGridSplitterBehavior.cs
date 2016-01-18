using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace ThorCyte.Infrastructure.Controls
{
    public class DoubleClickCollapseGridSplitterBehavior : Behavior<GridSplitter>
    {
        private double _oldSize = 0.0;

        protected override void OnAttached()
        {
            base.OnAttached();

            this.AssociatedObject.PreviewMouseDoubleClick +=
            (AssociatedObject_PreviewMouseDoubleClick);
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.PreviewMouseDoubleClick -=
            (AssociatedObject_PreviewMouseDoubleClick);
            base.OnDetaching();
        }

        void AssociatedObject_PreviewMouseDoubleClick
            (object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            GridSplitter splitter = sender as GridSplitter;
            Grid parent = splitter.Parent as Grid;

            int col = (int)splitter.GetValue(Grid.ColumnProperty);
            double width = parent.ColumnDefinitions[col + 1].ActualWidth;
            if (width > 0)
            {
                _oldSize = width;
                width = 0.0;
            }
            else
            {
                width = _oldSize;
            }

            parent.ColumnDefinitions[col + 1].SetValue
        (ColumnDefinition.WidthProperty, new GridLength(width));

        }
    }
}
