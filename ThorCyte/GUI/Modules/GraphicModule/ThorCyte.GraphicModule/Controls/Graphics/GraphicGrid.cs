using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ThorCyte.GraphicModule.Helper;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    public  class GraphicGrid : DrawingVisual
    {
        #region Properties and fields

        public List<double> XTicks { get; set; }

        public List<double> YTicks { get; set; }

        public bool IsShowXGrid { get; set; }

        public bool IsShowYGrid { get; set; }

        public Rect ClientRect { get; set; }

        private Color _objectColor;

        public Color ObjectColor
        {
            get { return _objectColor; }
            set
            {
                if (value == _objectColor)
                {
                    return;
                }
                _objectColor = value;
                Draw();
            }
        }

        #endregion

        #region Methods

        public void Draw()
        {
            using (var dc = RenderOpen())
            {
                DrawGird(dc);
            }
        }

        private void DrawGird(DrawingContext dc)
        {
            var brush = new SolidColorBrush(ObjectColor);
            var pen = new Pen(brush, 1);

            if (IsShowXGrid)
            {
                if (XTicks != null && XTicks.Count > 0)
                {
                    foreach (var tick in XTicks)
                    {
                        DrawHelper.DrawLine(dc, pen, new Point(tick, ClientRect.Bottom), new Point(tick, ClientRect.Top));
                    }
                }
            }

            if (IsShowYGrid)
            {
                if (YTicks != null && YTicks.Count > 0)
                {
                    foreach (var tick in YTicks)
                    {
                        DrawHelper.DrawLine(dc, pen, new Point(ClientRect.Left, tick), new Point(ClientRect.Right, tick));
                    }
                }
            }
        }

        #endregion
    }
}
