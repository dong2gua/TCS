using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    internal class GraphicsRectangle : GraphicsRectangleBase
    {
        #region Properties

        public override RegionType GraphicType
        {
            get { return RegionType.Rectangle; }
        }

        #endregion
        
        #region Constructors

        public GraphicsRectangle(double left, double top, double right, double bottom,
            double lineWidth, Color objectColor, double xScale, Size parentSize, string name, bool isDrawTrackerAll = true)
        {
            rectangleLeft = left;
            rectangleTop = top;
            rectangleRight = right;
            rectangleBottom = bottom;
            _graphicsLineWidth = lineWidth;
            _graphicsObjectColor = objectColor;
            _xScale = xScale;
            CreatedCanvasSize = parentSize;
            Name = name;
            IsDrawTrackerAll = isDrawTrackerAll;
        }

        #endregion Constructors

        #region Overrides

        public override void DrawTracker(DrawingContext dc)
        {
            if (IsDrawTrackerAll)
            {
                base.DrawTracker(dc);
            }
            else
            {
                DrawTrackerRectangle(dc,GetHandleRectangle(4));
                DrawTrackerRectangle(dc, GetHandleRectangle(8));
            }
        }

        /// <summary>
        /// Draw object
        /// </summary>
        public override void Draw(DrawingContext dc)
        {
            if (dc == null)
            {
                throw new ArgumentNullException("dc");
            }
            var brush = new SolidColorBrush(ObjectColor);
            var center = new Point((Rectangle.Left + Rectangle.Right) / 2, (Rectangle.Top + Rectangle.Bottom) / 2);
            var formatText = new FormattedText(Name, new CultureInfo("en-US"), FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Arial"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal), FontSize, new SolidColorBrush(Colors.Black));
            center.X -= formatText.Width / 2;
            center.Y -= formatText.Height / 2;
            DrawHelper.DrawRectangle(dc, _fillObjectBrush, new Pen(brush, ActualLineWidth), Rectangle);
            dc.DrawText(formatText, center);
            base.Draw(dc);
           }

        /// <summary>
        /// Test whether object contains point
        /// </summary>
        public override bool Contains(Point point)
        {
            return Rectangle.Contains(point);
        }

        public override string ToString()
        {
            return "Rectangle";
        }

        #endregion Overrides
    }
}
