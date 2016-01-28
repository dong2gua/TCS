using System.Windows;
using ThorCyte.GraphicModule.Controls.RegionTools;

namespace ThorCyte.GraphicModule.Controls
{
    public class Histogram : RegionCanvas
    {
        #region Constructor

        public Histogram()
        {
            Tools = new RegionTool[]
            {
                new  RegionToolPointer(),
                new  RegionToolRectangle()
            };
            CurrentTool = Tools[0];
        }

        #endregion

        #region Methods

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender,e);
            if (_isLoading)
            {
                InitGraphics();
            }
            UnSelectAll();
            _isLoading = false;
        }

        #endregion
    }
}
