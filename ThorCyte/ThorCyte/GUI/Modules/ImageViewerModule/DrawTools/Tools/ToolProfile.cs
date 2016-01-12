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
        public GraphicsProfile Profile { get; private set; }
        UpdateProfilePointsEvent _event;
        public ToolProfile()
        {
            Profile = new GraphicsProfile();
            _event = ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<UpdateProfilePointsEvent>();
            MemoryStream stream = new MemoryStream(Properties.Resources.CurProfile);
            ToolCursor = new Cursor(stream);
        }
        public override void OnMouseDown(DrawingCanvas drawingCanvas, MouseButtonEventArgs e, Point position)
        {
            var point = position;
            Profile.UpdatePoint(point, drawingCanvas);
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
            drawingCanvas.ReleaseMouseCapture();
            _event.Publish(new ProfilePoints() { StartPoint = new Point(Profile.Start.X * drawingCanvas.ActualScale.Item3, Profile.Start.Y * drawingCanvas.ActualScale.Item3), EndPoint = new Point(Profile.End.X * drawingCanvas.ActualScale.Item3, Profile.End.Y * drawingCanvas.ActualScale.Item3) });
        }
        protected static void AddNewObject(DrawingCanvas drawingCanvas, GraphicsBase o)
        {
            drawingCanvas.UnselectAll();
            o.Clip = new RectangleGeometry(new Rect(0, 0, drawingCanvas.ActualWidth / drawingCanvas.ActualScale.Item1, drawingCanvas.ActualHeight / drawingCanvas.ActualScale.Item2));
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
