using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.ImageViewerModule.DrawTools.Graphics;
using ThorCyte.ImageViewerModule.Events;

namespace ThorCyte.ImageViewerModule.DrawTools.Tools
{
    class ToolProfile : Tool
    {
        public GraphicsLine Profile { get; private set; }
        UpdateProfilePointsEvent _event;
        public ToolProfile()
        {
            Profile = new GraphicsLine();
            _event = ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<UpdateProfilePointsEvent>();
            MemoryStream stream = new MemoryStream(Properties.Resources.CurProfile);
            ToolCursor = new Cursor(stream);
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Profile.UpdatePoint(position, drawingCanvas);
                AddNewObject(drawingCanvas, Profile);
            }
        }
        public override void OnMouseMove(DrawingCanvas drawingCanvas, MouseEventArgs e, Point position)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!drawingCanvas.IsMouseCaptured) return;
                if (Profile != null)
                    Profile.MoveHandleTo(position, 2);
            }
        }
        public override void OnMouseUp(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            Profile.Normalize();
            drawingCanvas.ReleaseMouseCapture();
            _event.Publish(new ProfilePoints() { StartPoint = new Point(Profile.Start.X , Profile.Start.Y ), EndPoint = new Point(Profile.End.X , Profile.End.Y ) });
        }
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth , drawingCanvas.ActualHeight ));
            o.RefreshDrawing();
            if (!drawingCanvas.GraphicsList.Contains(o))
                drawingCanvas.GraphicsList.Add(o);
            drawingCanvas.CaptureMouse();
        }
        public override void SetCursor(DrawingCanvas drawingCanvas)
        {
            drawingCanvas.Cursor = ToolCursor;
        }
    }
}
