using System.Windows.Input;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    /// <summary>
    /// Base class for all drawing tools
    /// </summary>
    public abstract class RegionTool
    {
        public abstract void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e);

        public abstract void OnMouseMove(RegionCanvas graph, MouseEventArgs e);

        public abstract void OnMouseUp(RegionCanvas regionCanvas, MouseButtonEventArgs e);

        public abstract void SetCursor(RegionCanvas graph);
    }
}
