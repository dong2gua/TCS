using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.RegionTools
{
    /// <summary>
    /// Base class for all tools which create new graphic object
    /// </summary>
    public abstract class RegionToolObject : RegionTool
    {
        #region Properties and Fields

        public const double DefaultFontSize = 12.0;

        private Cursor _toolCursor;

        /// <summary>
        /// Tool cursor.
        /// </summary>
        protected Cursor ToolCursor
        {
            get { return _toolCursor; }
            set { _toolCursor = value; }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Left mouse is released.
        /// New object is created and resized.
        /// </summary>
        public override void OnMouseUp(RegionCanvas regionCanvas, MouseButtonEventArgs e)
        {
            if (regionCanvas.VisualList.Count == 0)
            {
                return;
            }
            if (regionCanvas.VisualList.Count > 0)
            {
                regionCanvas[regionCanvas.VisualList.Count - 1].Normalize();
            }

            var isRemove = false;
            var graphic = regionCanvas[regionCanvas.VisualList.Count - 1];
            var polygon = graphic as GraphicsPolygon;
            if (polygon != null)
            {
                if (polygon.Points.Length == 2)
                {
                    isRemove = true;
                    regionCanvas.VisualList.Remove(polygon);
                }
            }
            var rect = graphic as GraphicsRectangleBase;

            if (rect != null)
            {
                if (rect.Rectangle.Width < 5 && rect.Rectangle.Height < 5)
                {
                    isRemove = true;
                    regionCanvas.VisualList.Remove(rect);
                }
            }

            if (!isRemove)
            {
                regionCanvas.AddRegion(graphic);
            }

            regionCanvas.Tool = ToolType.Pointer;
            regionCanvas.Cursor = Cursors.None;
            regionCanvas.ReleaseMouseCapture();
            regionCanvas.UnSelectAll();
        }

        /// <summary>
        /// Add new object to drawing canvas.
        /// Function is called when user left-clicks drawing canvas,
        /// and one of ToolObject-derived tools is active.
        /// </summary>
        public static void AddNewObject(RegionCanvas graph, GraphicsBase o)
        {
            //var width = regionCanvas.BmpImage == null ? 200 : regionCanvas.BmpImage.Width;
            //var width = 200;
            graph.UnSelectAll();
            //o.FontSize = DefaultFontSize * graph.XScale;
            //o.CreatedBmpWidth = width;
            o.CreatedCanvasSize = graph.RenderSize;
            o.Clip = new RectangleGeometry(new Rect(0, 0, graph.ActualWidth, graph.ActualHeight));
           graph.VisualList.Add(o);
           graph.CaptureMouse();
        }

        /// <summary>
        /// Set cursor
        /// </summary>
        public override void SetCursor(RegionCanvas graph)
        {
            graph.Cursor = _toolCursor;
        }

        #endregion
    }
}
