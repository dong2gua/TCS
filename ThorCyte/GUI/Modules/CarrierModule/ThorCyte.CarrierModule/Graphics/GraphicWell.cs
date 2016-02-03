using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using ThorCyte.CarrierModule.Common;

namespace ThorCyte.CarrierModule.Graphics
{
    public class GraphicWell : DrawingVisual
    {
        #region Fileds

        private bool _isSelcted;
        private bool _isEnable;
        private bool _isFocused;
        private bool _isShowText;
        private double _actualScale;
        private Rect _wellRect;
        private int _textNumber;

        private SolidColorBrush _fillBrush;
        private SolidColorBrush _boundBrush;

        private readonly int _wellNo;
        private readonly double _margin;
        private readonly double _wellSize;
        private readonly int _wellRow;
        private readonly int _wellColumn;

        #endregion Fileds

        #region Properties

        public Rect WellRect
        {
            get
            {
                return _wellRect;
            }
        }

        public int TextNumber
        {
            set { _textNumber = value; }
            get { return _textNumber; }
        }

        public bool IsShowText
        {
            set { _isShowText = value; }
            get { return _isShowText; }
        }

        public double ActualScale
        {
            get { return _actualScale; }
            set
            {
                _actualScale = value;
                _wellRect.X = _margin + _actualScale * _wellSize * _wellColumn + 3;
                _wellRect.Y = _margin + _actualScale * _wellSize * _wellRow + 3;
                _wellRect.Width = _wellRect.Height = _actualScale * _wellSize - 6;
                RefreshDrawing();
            }
        }

        public bool IsSelected
        {
            set
            {
                if (_isEnable)
                {
                    _isSelcted = value; RefreshDrawing();
                }
            }
            get { return _isSelcted; }
        }

        public bool IsFocused
        {
            set { _isFocused = value; RefreshDrawing(); }
            get { return _isFocused; }
        }

        public SolidColorBrush FillBrush
        {
            set { _fillBrush = value; RefreshDrawing(); }
            get { return _fillBrush; }
        }

        public bool IsEnable
        {
            set { _isEnable = value; RefreshDrawing(); }
            get { return _isEnable; }
        }

        public int WellNo
        {
            get { return _wellNo; }
        }

        #endregion Properties

        #region Constructor
        public GraphicWell(int no, double margin, double size, int row, int col)
        {
            _wellNo = no;
            _margin = margin;
            _wellSize = size;
            _wellRow = row;
            _wellColumn = col;
        }
        #endregion

        #region Methods

        public void RefreshDrawing()
        {
            var dc = RenderOpen();

            Draw(dc);

            dc.Close();
        }

        private void Draw(DrawingContext drawingContext)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }

            var center = new Point(
                (_wellRect.Left + _wellRect.Right) / 2.0,
                (_wellRect.Top + _wellRect.Bottom) / 2.0);

            var radiusX = (_wellRect.Right - _wellRect.Left) / 2.0;
            var radiusY = (_wellRect.Bottom - _wellRect.Top) / 2.0;

            _boundBrush = _isEnable ? Brushes.White : Brushes.Gray;

            drawingContext.DrawEllipse(
                _fillBrush,
                new Pen(_boundBrush, 1),
                center,
                radiusX,
                radiusY);

            if (_isSelcted && _isEnable)
            {
                var pen = new Pen(Brushes.Red, 1);
                DrawFunction.DrawRectangle(drawingContext, null, pen, _wellRect);
            }

            if (_isFocused)
            {
                var pen = new Pen(Brushes.Black, 1);
                var pt0 = new Point();
                var pt1 = new Point();
                pt0.X = pt1.X = _wellRect.Left + _wellRect.Width / 2;
                pt0.Y = _wellRect.Top + 1;
                pt1.Y = _wellRect.Bottom - 1;
                DrawFunction.DrawLine(drawingContext, pen, pt0, pt1);

                pt0.Y = pt1.Y = _wellRect.Top + _wellRect.Height / 2;
                pt0.X = _wellRect.Left + 1;
                pt1.X = _wellRect.Right - 1;
                DrawFunction.DrawLine(drawingContext, pen, pt0, pt1);
            }

            if (_isShowText)
            {
                if (_wellRect.Width > 60)
                {
                    var formattedText = new FormattedText(_textNumber.ToString(CultureInfo.InvariantCulture),
                       CultureInfo.InvariantCulture,
                       FlowDirection.LeftToRight,
                       new Typeface("Verdana"),
                       12,
                       Brushes.White);

                    var pt0 = new Point();
                    double xpos = _textNumber.ToString(CultureInfo.InvariantCulture).Length * 4;
                    pt0.X = center.X - xpos;
                    pt0.Y = center.Y - 6;
                    drawingContext.DrawText(formattedText, pt0);
                }
            }

        }

        #endregion Methods
    }
}
