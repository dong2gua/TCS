using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.Control
{
    public  class SelectViewportCanvas:Canvas
    {
        public delegate void ClickHandler(int select);
        public event ClickHandler OnClick;
        private DrawingVisual _visual;
        private int _select=0;
        public SelectViewportCanvas()
        {
            Width = 50;
            Height = 50;
            MouseMove += SelectViewportCanvas_MouseMove;
            MouseUp += SelectViewportCanvas_MouseUp;
            _visual = new DrawingVisual();
            this.AddVisualChild(_visual);
            Refresh();
        }
        protected override Visual GetVisualChild(int index)
        {
            return _visual;
        }
        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }
        private void Refresh()
        {
            var drawingContext = _visual.RenderOpen();
            var pen = new Pen(Brushes.Black, 1);
            var selectPen = new Pen(Brushes.Red,2);
            drawingContext.DrawRectangle(null, _select != 0? selectPen:pen, new Rect(2, 2, 20, 20));
            drawingContext.DrawRectangle(null, _select == 2|| _select == 4 ? selectPen : pen, new Rect(27, 2, 20, 20));
            drawingContext.DrawRectangle(null, _select == 3|| _select == 4 ? selectPen : pen, new Rect(2, 27, 20, 20));
            drawingContext.DrawRectangle(null, _select == 4 ? selectPen : pen, new Rect(27, 27, 20, 20));
            drawingContext.Close();
        }
        private void SelectViewportCanvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            OnClick(_select);
        }
        private void SelectViewportCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            var point = e.GetPosition(this);
            if (point.X < 25 && point.Y < 25) _select = 1;
            else if (point.X >= 25 && point.Y < 25) _select = 2;
            else if (point.X < 25 && point.Y >= 25) _select = 3;
            else  _select = 4;
            Refresh();
        }
    }
}
