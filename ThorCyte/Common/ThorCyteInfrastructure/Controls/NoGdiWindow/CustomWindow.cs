

using System.Windows.Media;

namespace System.Windows.Controls
{
    [TemplatePart(Name = "rootGrid", Type = typeof(Grid))]
    public class CustomWindow : NoGdiWindow
    {

        public Visibility MinimizeButtonVisibility
        {
            get
            {
                if (this.ResizeMode == ResizeMode.NoResize)
                {
                    return Visibility.Collapsed;
                }

                return Visibility.Visible;
            }
        }

        public CustomWindow()
        {
            this.Style = Application.Current.Resources["CustomWindowStyle"] as Style;
        }

        protected override void ResizeableWindow_Initialized(object sender, EventArgs e)
        {
            // Visual Properties
            this.WindowStyle = System.Windows.WindowStyle.None;
            this.AllowsTransparency = true;
            this.Background = new SolidColorBrush(Colors.Transparent);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (this.ResizeMode == ResizeMode.NoResize)
            {
                return;
            }
            // Real transparency only works above Windows XP and if there are no WindowsFormsHost controls
            if ((Environment.OSVersion.Version.Major > 5) & (!this.UsesWindowsFormsHost))
            {
                Grid root = this.Template.FindName("rootGrid", this) as Grid;

                if (root != null)
                {
                    root.Children.Add(this.GetResizeHandleRectangle("TopDragHandle", HorizontalAlignment.Stretch, VerticalAlignment.Top));
                    root.Children.Add(this.GetResizeHandleRectangle("RightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Stretch));
                    root.Children.Add(this.GetResizeHandleRectangle("BottomDragHandle", HorizontalAlignment.Stretch, VerticalAlignment.Bottom));
                    root.Children.Add(this.GetResizeHandleRectangle("LeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Stretch));
                    root.Children.Add(this.GetResizeHandleRectangle("TopLeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Top));
                    root.Children.Add(this.GetResizeHandleRectangle("TopRightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Top));
                    root.Children.Add(this.GetResizeHandleRectangle("BottomRightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Bottom));
                    root.Children.Add(this.GetResizeHandleRectangle("BottomLeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Bottom));
                }
            }
        }
    }
}
