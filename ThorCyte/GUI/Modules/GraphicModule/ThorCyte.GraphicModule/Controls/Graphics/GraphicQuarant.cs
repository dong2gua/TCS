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

        public Scattergram ParentScattergram { get; private set; }

        #endregion

        public GraphicQuarant(Scattergram scattergram)
        {
            ParentScattergram = scattergram;
        }

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
            var scale = 100.0/EventPoints.Count;

            var brush = new SolidColorBrush(Colors.White);
            var percentValue = (EventPoints.Count == 0) ? 0 : (int)(FirstQuadrantPercent * scale + 0.5);
            var text = DrawHelper.GetFormatText(percentValue + "%", DefaultTitleFontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomRight.X - text.Width, ClientRect.Y));

            percentValue = (EventPoints.Count == 0) ? 0 : (int)(SecondQuadrantPercent * scale + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", DefaultTitleFontSize, brush);
            dc.DrawText(text, ClientRect.TopLeft);

            percentValue = (EventPoints.Count == 0) ? 0 : (int)(ThirdQuadrantPercent * scale + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", DefaultTitleFontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomLeft.X, ClientRect.BottomLeft.Y - text.Height));

            percentValue = (EventPoints.Count == 0) ? 0 : (int)(FourthQuadrantPercent * scale + 0.5);
            text = DrawHelper.GetFormatText(percentValue + "%", DefaultTitleFontSize, brush);
            dc.DrawText(text, new Point(ClientRect.BottomRight.X - text.Width, ClientRect.BottomLeft.Y - text.Height));

        }

        private void CalcQuadrantValues()
        {
            if (EventPoints == null || QuadrantLine == null)
            {
                return;
            }
            var xscale = ClientRect.Width / (ParentScattergram.Vm.XAxis.MaxValue - ParentScattergram.Vm.XAxis.MinValue);
            var yscale = ClientRect.Height / (ParentScattergram.Vm.YAxis.MaxValue - ParentScattergram.Vm.YAxis.MinValue);
            var centerX = QuadrantLine.HorizonLine.Start.X / xscale + ParentScattergram.Vm.XAxis.MinValue;
            var centerY = (ClientRect.Height - QuadrantLine.VerticalLine.Start.Y) / yscale + ParentScattergram.Vm.YAxis.MinValue;

            FirstQuadrantPercent = 0;
            SecondQuadrantPercent = 0;
            ThirdQuadrantPercent = 0;
            FourthQuadrantPercent = 0;

            foreach (var point in EventPoints)
            {
                if (point.X >= centerX)
                {
                    if (point.Y <= centerY)
                    {
                        ++FourthQuadrantPercent;
                    }
                    else
                    {
                        ++FirstQuadrantPercent;
                      
                    }
                }
                else
                {
                    if (point.Y < centerY)
                    {
                        ++ThirdQuadrantPercent;
                    }
                    else if (point.Y > centerY)
                    {
                        ++SecondQuadrantPercent;
                    }
                }
            }
        }

        #endregion
    }
}
