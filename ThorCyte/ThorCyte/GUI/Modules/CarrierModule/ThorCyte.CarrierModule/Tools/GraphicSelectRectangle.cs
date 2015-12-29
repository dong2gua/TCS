using System.Windows;
using System.Windows.Media;
using ThorCyte.CarrierModule.Common;

namespace ThorCyte.CarrierModule.Tools
{
    public class GraphicSelectRectangle : DrawingVisual
    {
        #region Fileds

        private double _rectangleLeft;
        private double _rectangleTop;
        private double _rectangleRight;
        private double _rectangleBottom;

        #endregion

        #region Properties

        public double Left
        {
            get { return _rectangleLeft; }
            set { _rectangleLeft = value; }
        }

        public double Top
        {
            get { return _rectangleTop; }
            set { _rectangleTop = value; }
        }

        public double Right
        {
            get { return _rectangleRight; }
            set { _rectangleRight = value; }
        }

        public double Bottom
        {
            get { return _rectangleBottom; }
            set { _rectangleBottom = value; }
        }

        public Rect Rectangle
        {
            get
            {
                double l, t, w, h;
                if (_rectangleLeft <= _rectangleRight)
                {
                    l = _rectangleLeft;
                    w = _rectangleRight - _rectangleLeft;
                }
                else
                {
                    l = _rectangleRight;
                    w = _rectangleLeft - _rectangleRight;
                }

                if (_rectangleTop <= _rectangleBottom)
                {
                    t = _rectangleTop;
                    h = _rectangleBottom - _rectangleTop;
                }
                else
                {
                    t = _rectangleBottom;
                    h = _rectangleTop - _rectangleBottom;
                }

                return new Rect(l, t, w, h);
            }
        }

        #endregion

        #region Constructor

        public GraphicSelectRectangle(double left, double top, double right, double bottom)
        {
            _rectangleLeft = left;
            _rectangleTop = top;
            _rectangleRight = right;
            _rectangleBottom = bottom;
        }

        #endregion

        #region Methods

        public void Draw(DrawingContext drawingContext)
        {
            DrawFunction.DrawRectangle(drawingContext,
                null,
                new Pen(Brushes.White, 1),
                Rectangle);

            var dashStyle = new DashStyle();
            dashStyle.Dashes.Add(4);

            var dashedPen = new Pen(Brushes.Black, 1) { DashStyle = dashStyle };

            DrawFunction.DrawRectangle(drawingContext,
                null,
                dashedPen,
                Rectangle);
        }

        public void RefreshDrawing()
        {
            var dc = RenderOpen();

            Draw(dc);

            dc.Close();
        }

        #endregion
    }
}
