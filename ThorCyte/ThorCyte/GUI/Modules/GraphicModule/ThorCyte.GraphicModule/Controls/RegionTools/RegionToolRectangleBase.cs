using System.Windows.Input;
using ThorCyte.GraphicModule.Controls.Graphics;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    public class RegionToolRectangleBase : RegionToolObject
    {
        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {

        }

        /// <summary>
        /// Set cursor and resize new object.
        /// </summary>
        public override void OnMouseMove(RegionCanvas graph, MouseEventArgs e)
        {
            graph.Cursor = ToolCursor;
            var point = e.GetPosition(graph);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (graph.VisualList.Count > 0)
                {
                    var graphicRect = graph[graph.VisualList.Count - 1] as GraphicsRectangleBase;
                    if (graphicRect == null)
                    {
                        return;
                    }
                    if (graphicRect.IsDrawTrackerAll)
                    {
                        graphicRect.MoveHandleTo(point, 5);
                    }
                    else
                    {
                        graphicRect.Right = point.X;
                        graphicRect.RefreshDrawing();
                    }
                }
            }
        }
    }
}
