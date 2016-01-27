using System.Windows;
using System.Windows.Input;
using ROIService;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Helper;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    class RegionToolPolygon:RegionToolObject
    {
        #region Fields

        private GraphicsPolygon _newPolygon;

        #endregion

        #region Methods

        /// <summary>
        /// Create new object
        /// </summary>
        public override void OnMouseDown(RegionCanvas graph, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(graph);
            if (!_isNew)
            {
                var name = ConstantHelper.PrefixRegionName + ROIManager.Instance.GetRegionId();
                _newPolygon = new GraphicsPolygon(
                     new[] { p, new Point(p.X + 1, p.Y + 1) },
                     graph.LineWidth, graph.ObjectColor, graph.RenderSize, name);
                AddNewObject(graph, _newPolygon);
                _isNew = true;
            }
            else
            {
                if (_newPolygon == null)
                    return;

                if (e.ChangedButton == MouseButton.Right)
                {
                    _isNew = false;
                }
                else
                {
                    if (graph.IsPointInCanvas(p))
                    {
                        _newPolygon.AddPoint(p);
                    }

                }
            }
        }

        /// <summary>
        /// Set cursor and resize new polyline
        /// </summary>
        public override void OnMouseMove(RegionCanvas graph, MouseEventArgs e)
        {
            graph.Cursor = ToolCursor;

            if (_newPolygon == null)
            {
                return; // precaution
            }

            var p = e.GetPosition(graph);
            _newPolygon.MoveHandleTo(p, _newPolygon.HandleCount);
        }

        public override void OnMouseUp(RegionCanvas graph, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                _newPolygon = null;
                _isNew = false;
                base.OnMouseUp(graph, e);
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                //var p = e.GetPosition(graph);
                //if (!graph.IsPointInCanvas(p))
                //{
                //    _newPolygon = null;
                //    _isNewPolygon = false;
                //    base.OnMouseUp(graph, e);
                //}
            }
        }

        #endregion
    }
}
