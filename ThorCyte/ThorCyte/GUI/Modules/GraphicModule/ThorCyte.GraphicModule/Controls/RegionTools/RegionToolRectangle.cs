using System.Windows.Input;
using ROIService;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Helper;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    public class RegionToolRectangle : RegionToolRectangleBase
    {
        /// <summary>
        /// Create new rectangle
        /// </summary>
        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(graph);
            var name = ConstantHelper.PrefixRegionName + ROIManager.Instance.GetRegionId();
            var isDrawTrackerAll = (graph as Scattergram) != null;
            var startY = isDrawTrackerAll ? p.Y : graph.EndYPoint.Y;
            var endY = isDrawTrackerAll ? p.Y + 1 : graph.OriginalPoint.Y;
            var rect = new GraphicsRectangle(p.X, startY, p.X + 1, endY, graph.LineWidth, graph.ObjectColor, 1, graph.RenderSize, name, isDrawTrackerAll);
            rect.OriginalPoint = p;
            AddNewObject(graph, rect);
        }
    }
}
