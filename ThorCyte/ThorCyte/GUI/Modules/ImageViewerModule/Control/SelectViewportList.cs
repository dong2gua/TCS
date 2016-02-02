using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.Control
{
    public class SelectViewportList : StackPanel
    {
        public delegate void ClickHandler(int select);
        public event ClickHandler OnClick;
        public SelectViewportList()
        {
            ListView lv = new ListView();
            lv.Items.Add("One Viewport");
            lv.Items.Add("Horizontal Viewport");
            lv.Items.Add("Vertical Viewport");
            lv.Items.Add("All Viewport");
            lv.SelectedItem = null;
            lv.SelectionChanged += Lv_SelectionChanged;
            this.Children.Add(lv);
        }

        private void Lv_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OnClick == null) return;
            var lv = sender as ListView;
            if (lv == null) return;
            OnClick(lv.SelectedIndex+1);
        }
    }
}
