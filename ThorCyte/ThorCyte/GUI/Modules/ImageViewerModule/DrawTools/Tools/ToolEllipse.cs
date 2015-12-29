﻿using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolEllipse : ToolObject
    {
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;

            AddNewObject(drawingCanvas,
                new GraphicsEllipse(
                    point.X,
                    point.Y,
                    point.X + 1,
                    point.Y + 1,
                    drawingCanvas.ActualScale));
        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {

                if (!drawingCanvas.IsMouseCaptured) return;
                if (drawingCanvas.Count > 0)
                {
                    drawingCanvas[drawingCanvas.Count - 1].MoveHandleTo(
                        position, 5);
                }
            }
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            base.OnMouseUp(drawingCanvas, e, position);
        }

    }
}
