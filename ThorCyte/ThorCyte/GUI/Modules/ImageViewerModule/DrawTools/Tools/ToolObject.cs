using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using System;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    abstract class ToolObject : Tool
    {
        protected Cursor ToolCursor { get; set; }


        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (drawingCanvas.Count > 0)
            {
                var obj= drawingCanvas[drawingCanvas.Count - 1];
                obj.Normalize();
            }

            drawingCanvas.Tool = ToolType.Pointer;
            drawingCanvas.Cursor = Cursors.Arrow;
            drawingCanvas.ReleaseMouseCapture();

        }

        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth / drawingCanvas.ActualScale.Item1, drawingCanvas.ActualHeight / drawingCanvas.ActualScale.Item2));

            o.IsSelected = true;
            drawingCanvas.GraphicsList.Add(o);
            drawingCanvas.CaptureMouse();
        }


    }
}
