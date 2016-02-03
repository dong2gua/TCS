using System.Windows.Input;
using ROIService;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Helper;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    class RegionToolEllipse:RegionToolRectangleBase
    {
        /// <summary>
        /// Create new rectangle
        /// </summary>
        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(graph);
            var name = ConstantHelper.PrefixRegionName + ROIManager.Instance.GetRegionId();
            AddNewObject(graph, new GraphicsEllipse(p.X,p.Y, p.X + 1, p.Y + 1, graph.LineWidth, graph.ObjectColor, 1, graph.RenderSize, name));
            _isNew = true;
        }
    }
}
