using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls.Graphics
{
    /// <summary>
    /// Base class for all graphics objects.
    /// </summary>
    public abstract class GraphicsBase : DrawingVisual
    {
        #region Constructor

        protected GraphicsBase()
        {
            Id = GetHashCode();
            FontSize = DefaultFontSize;
        }

        #endregion Constructor

        #region Properties and Fields

        public double DefaultFontSize = 12.0;
        protected const double HitTestWidth = 8.0;
        protected const double HandleSize = 12.0;
        protected double _graphicsLineWidth;
        protected SolidColorBrush _fillObjectBrush = null;
        
        protected virtual double ActualLineWidth
        {
            get { return 1.0; }
        }

        protected double LineHitTestWidth
        {
            get
            {
                // Ensure that hit test area is not too narrow
                return Math.Max(8.0, ActualLineWidth);
             }
        }

        //public double CreatedCanvasWidth { set; get; }

        public Size CreatedCanvasSize { set; get; }

  //      public double CreatedBmpWidth { set; get; }

        public double GraphicsLeft { get; set; }

        public double GraphicsTop { get; set; }

        public double GraphicsRight { get; set; }

        public double GraphicsBottom { get; set; }

        public Point OriginalPoint { get; set; }

        public double FontSize { get; set; }

        public string Name { get; set; }

        public int Id { get; set; }

        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                _isSelected = value;
                RefreshDrawing();
            }
        }

        protected Color _graphicsObjectColor;

        public Color ObjectColor
        {
            get { return _graphicsObjectColor; }
            set
            {
                _graphicsObjectColor = value;
                RefreshDrawing();
            }
        }

        protected double _xScale = 1.0;

        public double XScale
        {
            get { return _xScale; }
            set
            {
                var oldXScale = _xScale;
                _xScale = value;
                XScaleChanged(_xScale / oldXScale);
                RefreshDrawing();
            }
        }


        protected double _yScale = 1.0;

        public double YScale
        {
            get { return _yScale; }
            set
            {
                var oldYScale = _yScale;
                _yScale = value;
                YScaleChanged(_yScale / oldYScale);
                RefreshDrawing();
            }
        }

        /// <summary>
        /// Returns number of handles
        /// </summary>
        public abstract int HandleCount { get;}
        
        public abstract RegionType GraphicType { get; }

        #endregion Properties

        #region Abstract Methods and Properties
        
        /// <summary>
        /// Hit test, should be overwritten in derived classes.
        /// </summary>
        public abstract bool Contains(Point point);

        /// <summary>
        /// Create object for serialization
        /// </summary>
        //public abstract PropertiesGraphicsBase CreateSerializedObject();

        /// <summary>
        /// Get handle point by 1-based number
        /// </summary>
        public abstract Point GetHandle(int handleNumber);

        /// <summary>
        /// Hit test.
        /// Return value: -1 - no hit
        ///                0 - hit anywhere
        ///                > 1 - handle number
        /// </summary>
        public abstract int MakeHitTest(Point point);


        /// <summary>
        /// Test whether object intersects with rectangle
        /// </summary>
        public abstract bool IntersectsWith(Rect rectangle);

        /// <summary>
        /// Move object
        /// </summary>
        public abstract void Move(double deltaX, double deltaY);

        /// <summary>
        /// Move handle to the point
        /// </summary>
        public abstract void MoveHandleTo(Point point, int handleNumber);

        /// <summary>
        /// Get cursor for the handle
        /// </summary>
        public abstract Cursor GetHandleCursor(int handleNumber);

        #endregion Abstract Methods and Properties

        #region Virtual Methods

        /// <summary>
        /// Normalize object.
        /// Call this function in the end of object resizing,
        /// </summary>
        public virtual void Normalize() { }

        public virtual void UpdateRegion(Point basePoint) { }
        
        /// <summary>
        /// Implements actual drawing code.
        /// 
        /// Call GraphicsBase.Draw in the end of every derived class Draw 
        /// function to draw tracker if necessary.
        /// </summary>
        public virtual void Draw(DrawingContext dc)
        {
            if (IsSelected )
            {
                DrawTracker(dc);
            }
        }
        
        /// <summary>
        /// Draw tracker for _isSelected object.
        /// </summary>
        public virtual void DrawTracker(DrawingContext dc)
        {
            for (var i = 1; i <= HandleCount; i++)
            {
                DrawTrackerRectangle(dc, GetHandleRectangle(i));
            }
        }

        public virtual void XScaleChanged(double rate) { }

        public virtual void YScaleChanged(double yRate) { }

        #endregion Virtual Methods

        #region Other Methods

        /// <summary>
        /// Draw tracker rectangle
        /// </summary>
       protected  void DrawTrackerRectangle(DrawingContext dc, Rect rectangle)
        {
            // Internal
            var rect = new Rect(rectangle.Left + rectangle.Width/4, rectangle.Top + rectangle.Height/4,
                rectangle.Width/2, rectangle.Height/2);
            DrawHelper.DrawRectangle(dc, new SolidColorBrush(_graphicsObjectColor),null,rect);
        }


        /// <summary>
        /// Refresh drawing.
        /// Called after change if any object property.
        /// </summary>
        public void RefreshDrawing()
        {
            var dc = RenderOpen();
            Draw(dc);
            dc.Close();
        }

        /// <summary>
        /// Get handle rectangle by 1-based number
        /// </summary>
        public Rect GetHandleRectangle(int handleNumber)
        {
            var point = GetHandle(handleNumber);

            var size = HandleSize * _xScale;
            if (size < 9.0)
            {
                size = 9.0;
            }
            else if (size > 24.0)
            {
                size = 24.0;
            }

            return new Rect(point.X - size / 2, point.Y - size / 2,size, size);
        }

        #endregion Other Methods

        #region overrides

        public override string ToString()
        {
            return "BaseGraphics";
        }

        #endregion
    }
}
