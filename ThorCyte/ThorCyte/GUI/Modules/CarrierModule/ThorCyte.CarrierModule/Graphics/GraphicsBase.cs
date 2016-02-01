using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.CarrierModule.Common;

namespace ThorCyte.CarrierModule.Graphics
{
    /// <summary>
    /// Base class for all graphics objects.
    /// </summary>
    public abstract class GraphicsBase : DrawingVisual
    {
        #region Fields

        protected double GraphicsLineWidth;
        protected Color GraphicsObjectColor;
        protected double GraphicsActualScale;
        protected bool Selected;
        protected bool Locked;
        protected SolidColorBrush FillObjectBrush;

        protected const double HitTestWidth = 8.0;
        protected const double HandleSize = 12.0;

        // external rectangle
        static readonly SolidColorBrush SlBrush = new SolidColorBrush(Colors.HotPink);
        public int RoomId;

        #endregion Class Members

        #region Constructor

        protected GraphicsBase()
        {
            FillObjectBrush = null;
            Id = GetHashCode();
            IsLocked = true;
        }

        #endregion Constructor

        #region Properties

        public bool IsSelected
        {
            get { return Selected; }
            set
            {
                Selected = value;
                if (Selected && Locked)
                {
                    FillObjectBrush = SlBrush;
                }
                else
                {
                    FillObjectBrush = null;
                }
                RefreshDrawing();
            }
        }

        public bool IsLocked
        {
            get { return Locked; }
            set
            {
                Locked = value;
                if (Selected && Locked)
                {
                    FillObjectBrush = SlBrush;
                }
                else
                {
                    FillObjectBrush = null;
                }
                RefreshDrawing();
            }
        }

        public Color FillObjectColor
        {
            get { return Colors.HotPink; }
        }

        public Color ObjectColor
        {
            get
            {
                return GraphicsObjectColor;
            }

            set
            {
                GraphicsObjectColor = value;

                RefreshDrawing();
            }
        }

        public double ActualScale
        {
            get
            {
                return GraphicsActualScale;
            }

            set
            {
                var oldScale = GraphicsActualScale;
                GraphicsActualScale = value;
                Scale(GraphicsActualScale / oldScale);
                RefreshDrawing();
            }
        }

        protected double ActualLineWidth
        {
            get
            {
                return 1.0;
            }
        }

        protected double LineHitTestWidth
        {
            get
            {
                // Ensure that hit test area is not too narrow
                return Math.Max(8.0, ActualLineWidth);
            }
        }



        public int Id { get; set; }

        public double GraphicsLeft { get; set; }

        public double GraphicsTop { get; set; }

        public double GraphicsRight { get; set; }

        public double GraphicsBottom { get; set; }

        #endregion Properties

        #region Abstract Methods and Properties

        /// <summary>
        /// Returns number of handles
        /// </summary>
        public abstract int HandleCount
        {
            get;
        }


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
        public virtual void Normalize()
        {
            // Empty implementation is OK for classes which don't require
            // normalization, like line.
            // Normalization is required for rectangle-based classes.
        }

        /// <summary>
        /// Implements actual drawing code.
        /// 
        /// Call GraphicsBase.Draw in the end of every derived class Draw 
        /// function to draw tracker if necessary.
        /// </summary>
        public virtual void Draw(DrawingContext drawingContext)
        {
            if (IsSelected && !IsLocked)
            {
                DrawTracker(drawingContext);
            }
        }


        /// <summary>
        /// Draw tracker for selected object.
        /// </summary>
        public virtual void DrawTracker(DrawingContext drawingContext)
        {
            for (int i = 1; i <= HandleCount; i++)
            {
                DrawTrackerRectangle(drawingContext, GetHandleRectangle(i));
            }
        }

        public virtual void Scale(double rate) { }



        #endregion Virtual Methods

        #region Other Methods

        /// <summary>
        /// Draw tracker rectangle
        /// </summary>
        private void DrawTrackerRectangle(DrawingContext drawingContext, Rect rectangle)
        {

            DrawFunction.DrawRectangle(drawingContext, new SolidColorBrush(GraphicsObjectColor), null,
                new Rect(rectangle.Left + rectangle.Width / 3,
                 rectangle.Top + rectangle.Height / 3,
                 rectangle.Width / 3,
                 rectangle.Height / 3));
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
            Point point = GetHandle(handleNumber);

            double size = HandleSize * GraphicsActualScale;
            if (size < 9.0)
            {
                size = 9.0;
            }
            else if (size > 24.0)
            {
                size = 24.0;
            }

            return new Rect(point.X - size / 2, point.Y - size / 2,
                size, size);
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
