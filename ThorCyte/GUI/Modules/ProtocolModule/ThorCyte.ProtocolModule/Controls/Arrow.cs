using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ThorCyte.ProtocolModule.Controls
{
    /// <summary>
    /// Defines a simple straight arrow draw along a line.
    /// </summary>
    public class Arrow : ListBoxItem
    {
        #region Dependency Property
        public static readonly DependencyProperty StartProperty =
            DependencyProperty.Register("Start", typeof(Point), typeof(Arrow),
                new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty EndProperty =
            DependencyProperty.Register("End", typeof(Point), typeof(Arrow),
                new FrameworkPropertyMetadata(new Point(0.0, 0.0), FrameworkPropertyMetadataOptions.AffectsRender));

        public static readonly DependencyProperty ZIndexProperty =
            DependencyProperty.Register("ZIndex", typeof(int), typeof(Arrow),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ParentPannelViewProperty =
            DependencyProperty.Register("ParentPannelView", typeof(PannelView), typeof(Arrow),
                new FrameworkPropertyMetadata(ParentNetworkView_PropertyChanged));


        public static readonly DependencyProperty ConBrushProperty =
            DependencyProperty.Register("ConBrush", typeof(Brush), typeof(Arrow),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion

        #region Properties and Fields
        private bool _isLeftMouseDown;
        private bool _isLeftMouseAndControlDown;

        /// <summary>
        /// The start point of the arrow.
        /// </summary>
        public Point Start
        {
            get { return (Point)GetValue(StartProperty); }
            set { SetValue(StartProperty, value); }
        }

        /// <summary>
        /// The end point of the arrow.
        /// </summary>
        public Point End
        {
            get { return (Point)GetValue(EndProperty); }
            set { SetValue(EndProperty, value); }
        }

        /// <summary>
        /// The Z index of the _module.
        /// </summary>
        public int ZIndex
        {
            get { return (int)GetValue(ZIndexProperty); }
            set { SetValue(ZIndexProperty, value); }
        }


        /// <summary>
        /// The Arrow fill brush.
        /// </summary>
        public Brush ConBrush
        {
            get { return (Brush)GetValue(ConBrushProperty); }
            set { SetValue(ConBrushProperty, value); }
        }


        /// <summary>
        /// Reference to the data-bound parent NetworkView.
        /// </summary>
        public PannelView ParentPannelView
        {
            get { return (PannelView)GetValue(ParentPannelViewProperty); }
            set { SetValue(ParentPannelViewProperty, value); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Bring the _module to the front of other elements.
        /// </summary>
        internal void BringToFront()
        {
            if (ParentPannelView == null)
            {
                return;
            }
            var maxZ = ParentPannelView.FindMaxZIndex();
            ZIndex = maxZ + 1;
        }

        private static void ParentNetworkView_PropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Bring new _nodes to the front of the z-order.
            var nodeItem = (Arrow)d;
            nodeItem.BringToFront();
        }


        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseDown(e);

            BringToFront();

            if (ParentPannelView != null)
            {
                ParentPannelView.Focus();
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                _isLeftMouseDown = true;
                LeftMouseDownSelectionLogic();
                e.Handled = true;
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
            }
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isLeftMouseDown)
            {
                // Execute mouse up selection logic only if there was no drag operation.
                LeftMouseUpSelectionLogic();
                _isLeftMouseDown = false;
                e.Handled = true;
            }
        }

        private void LeftMouseDownSelectionLogic()
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0)
            {
                // Control key was held down.
                // This means that the rectangle is being added to or removed from the existing selection.
                // Don't do anything yet, we will act on this later in the MouseUp event handler.
                _isLeftMouseAndControlDown = true;
                IsSelected = !IsSelected;
            }
            else
            {
                // Control key is not held down.
                _isLeftMouseAndControlDown = false;
            }
        }


        private void LeftMouseUpSelectionLogic()
        {
            if (_isLeftMouseAndControlDown)
            {
            }
            else
            {
                IsSelected = true;
            }
            _isLeftMouseAndControlDown = false;
        }

        #endregion
    }
}
