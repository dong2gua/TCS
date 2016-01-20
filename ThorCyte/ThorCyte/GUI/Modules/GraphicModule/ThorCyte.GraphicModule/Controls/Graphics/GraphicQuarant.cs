using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ThorCyte.GraphicModule.Controls.RegionTools;
using ThorCyte.GraphicModule.Helper;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    public class GraphicQuarant : DrawingVisual
    {
        #region Properies and Fields

        public const double DefaultTitleFontSize = 12.0;

        public int FirstQuadrantPercent { get; set; }

        public int SecondQuadrantPercent { get; set; }

        public int ThirdQuadrantPercent { get; set; }

        public int FourthQuadrantPercent { get; set; }

        public List<Point> EventPoints { get; set; }

        public Color ObjectColor { get; set; }

        public RegionToolLine QuadrantLine { get; set; }

        public Rect ClientRect { get; set; }

        public Rect BaseRect { get; set; }

        private bool _isShow;

        public bool IsShow
        {
            get { return _isShow; }
            set
            {
                _isShow = value;
                ShowQuadrant(_isShow);
            }
        }

        #endregion

        #region Methods

        public void Draw()
        {
            CalcQuadrantValues();
            using (var dc = RenderOpen())
            {
                DrawQuadrant(dc);
            }
        }

        private void ShowQuadrant(bool value)
        {
            QuadrantLine.HorizonLine.IsShow = value;
            QuadrantLine.VerticalLine.IsShow = value;
            Draw();
        }

        private void DrawQuadrant(DrawingContext dc)
        {
            if (!IsShow || EventPoints == null)
                return;
            
            var brush = new SolidColorBrush(ObjectColor);
            var count = EventPoints.Count;
            var percentValue = (count == 0) ? 0 : (int)(FirstQuadrantPercent * 100.0 / count + 0.5);
            var fontSize = DefaultTitleFontSize;
            var text = DrawHelper.GetFormatText(percentValue + "%", fontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomRight.X - text.Width, ClientRect.Y));

            percentValue = (count == 0) ? 0 : (int)(SecondQuadrantPercent * 100.0 / count + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", fontSize, brush);
            dc.DrawText(text, ClientRect.TopLeft);

            percentValue = (count == 0) ? 0 : (int)(ThirdQuadrantPercent * 100.0 / count + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", fontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomLeft.X, ClientRect.BottomLeft.Y - text.Height));

            percentValue = (count == 0) ? 0 : (int)(FourthQuadrantPercent * 100.0 / count + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", fontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomRight.X - text.Width, ClientRect.BottomLeft.Y - text.Height));

        }

        private void CalcQuadrantValues()
        {
            if (EventPoints == null || QuadrantLine == null)
            {
                return;
            }
            var centerX = QuadrantLine.HorizonLine.Start.X ;
            var centerY = QuadrantLine.VerticalLine.Start.Y ;
            var xscale = ClientRect.Width/BaseRect.Width;
            var yscale = ClientRect.Height/BaseRect.Height;
            FirstQuadrantPercent = 0;
            SecondQuadrantPercent = 0;
            ThirdQuadrantPercent = 0;
            FourthQuadrantPercent = 0;

            foreach (var point in EventPoints)
            {
                var x = point.X*xscale;
                var y = point.Y*yscale;

                if (x >= centerX)
                {
                    if (y <= centerY)
                    {
                        FirstQuadrantPercent++;
                    }
                    else
                    {
                        FourthQuadrantPercent++;
                    }
                }
                else
                {
                    if (y < centerY)
                    {
                        SecondQuadrantPercent++;
                    }
                    else if (y > centerY)
                    {
                        ThirdQuadrantPercent++;
                    }
                }
            }
        }

        #endregion
    }
}
