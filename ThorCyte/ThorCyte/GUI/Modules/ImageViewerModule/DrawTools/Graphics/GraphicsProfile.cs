﻿using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.DrawTools.Graphics
{
    public class GraphicsProfile : GraphicsLine
    {
        public GraphicsProfile()
        {
        }
        public void UpdatePoint(Point point, DrawingCanvas canvas)
        {
            Canvas = canvas;
            ActualScale = Canvas.ActualScale;
            point = VerifyPoint(point);
            LineStart = point;
            LineEnd = point;
            RectangleLeft = point.X;
            RectangleTop = point.Y;
            RectangleRight = point.X;
            RectangleBottom = point.Y;
            GraphicsObjectColor = Colors.DeepPink;
        }
    }
}
