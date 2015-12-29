using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using System;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.ImageViewerModule.Events;
using Prism.Events;
namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolProfile : Tool
    {
      public  GraphicsProfile Profile { get; private set; }
        UpdateProfilePointsEvent _event;
        public ToolProfile()
        {
            Profile = new GraphicsProfile();
            _event= ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<UpdateProfilePointsEvent>();
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;
            Profile.UpdatePoint(point, new Point(point.X + 1, point.Y + 1), drawingCanvas.ActualScale);
            AddNewObject(drawingCanvas, Profile);

        }

        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            var point = position;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!drawingCanvas.IsMouseCaptured) return;
                if (Profile != null)
                {
                    Profile.MoveHandleTo(point, 2);
                }

            }
        }

        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (drawingCanvas.Count > 0)
            {
                var obj = drawingCanvas[drawingCanvas.Count - 1];
                obj.Normalize();
            }

            //drawingCanvas.Tool = ToolType.Pointer;
            drawingCanvas.Cursor = Cursors.Arrow;
            drawingCanvas.ReleaseMouseCapture();
            _event.Publish(new ProfilePoints() { StartPoint =new Point( Profile.Start.X-drawingCanvas.GraphicsImage.Rectangle.X,Profile.Start.Y-drawingCanvas.GraphicsImage.Rectangle.Y), EndPoint = new Point(Profile.End.X - drawingCanvas.GraphicsImage.Rectangle.X, Profile.End.Y - drawingCanvas.GraphicsImage.Rectangle.Y) });
        }
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(drawingCanvas.LimitX, drawingCanvas.LimitY, Math.Min(drawingCanvas.ActualWidth / drawingCanvas.ActualScale.Item1, drawingCanvas.GraphicsImage.Rectangle.Width), Math.Min(drawingCanvas.ActualHeight / drawingCanvas.ActualScale.Item2, drawingCanvas.GraphicsImage.Rectangle.Height)));
            o.RefreshDrawing();

            if (!drawingCanvas.GraphicsList.Contains(o))
                drawingCanvas.GraphicsList.Add(o);
            drawingCanvas.CaptureMouse();
        }

        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = Cursors.IBeam;
        }

    }
}
