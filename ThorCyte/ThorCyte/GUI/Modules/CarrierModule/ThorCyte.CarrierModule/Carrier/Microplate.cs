using System;
using System.Collections;
using System.Windows;
using System.Windows.Shapes;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{ 
    /// <summary>
    /// Summary description for Plate.
    /// </summary>
    public class Microplate : Carrier
    {
        #region Fileds
        private int _threshold = 3;
        public ScanPathType ScanPath = ScanPathType.Serpentine;
        private RegionShape _scanAreaShape = RegionShape.Rectangle;
        private bool _isRectangleShape;
        private int _saRowCount;
        private int _saRegionWidth;
        private int _saRegionHeight;

        private int _saColCount;
        private int _scanAreaRadius;

        #endregion

        #region Properties

        public int RowCount { get { return ((MicroplateDef)_carrierDef).NumRows; } }
        public int ColumnCount { get { return ((MicroplateDef)_carrierDef).NumColumns; } }

        public int RegionCount
        {
            get { return _activeRegions.Count; }
        }

        public double Interval { get { return ((MicroplateDef)_carrierDef).Interval; } }

        public double WellSize{
            get { return ((MicroplateDef) _carrierDef).WellSize; }
        }

        public RegionShape ScanAreaShape { get { return _scanAreaShape; } }

        public int ScanAreaRadius
        {
            set
            { SetProperty(ref _scanAreaRadius, value); }
            get { return _scanAreaRadius; }
        }

        public int SaRowCount
        {
            set
            {
                SetProperty(ref _saRowCount, value);
            }
            get { return _saRowCount; }
        }

        public int SaColCount
        {
            set
            {
                SetProperty(ref _saColCount, value);
            }
            get { return _saColCount; }
        }

        public int SaRegionWidth
        {
            set
            {
                SetProperty(ref _saRegionWidth, value);
            }
            get { return _saRegionWidth; }
        }

        public int SaRegionHeight
        {
            set
            {
                SetProperty(ref _saRegionHeight, value);
            }
            get { return _saRegionHeight; }
        }

        public bool IsRectangleShape
        {
            set
            {
                SetProperty(ref _isRectangleShape, value);
            }
            get { return _isRectangleShape; }
        }

        #endregion

        #region Constructor

        public Microplate(CarrierDef carrierDef)
            : base(carrierDef)
        {
            Calibrate(StageRef);
        }

        #endregion

        #region Method

        public void SetCircularScanArea(int radius)
        {
            IsRectangleShape = false;
            _scanAreaShape = RegionShape.Ellipse;
            ScanAreaRadius = radius;
        }

        public void SetRectangularScanArea(int rows, int cols)
        {
            IsRectangleShape = true;
            _scanAreaShape = RegionShape.Rectangle;
            SaRowCount = rows;
            SaColCount = cols;
        }

        // get bounding rect of the specified well
        public Rect GetBounds(int rid)
        {
            return this[rid].Bound;
        }

        // find the region from the carrier coord
        public override ScanRegion RegionFromPoint(Point pt)
        {
            foreach (var sr in TotalRegions)
            {
                if (sr.ScanRegionShape != RegionShape.Polygon)
                {
                    if (sr.Bound.Contains(pt))
                    {
                        return sr;
                    }
                }
                else
                {
                    //is polygon
                    var pg = new Polygon();
                    foreach (var spt in sr.Points)
                        pg.Points.Add(spt);

                    var gm = pg.Clip;

                    if (gm.FillContains(pt))
                    {
                        return sr;
                    }
                }
            }

            return null;
        }

        public override CarrierRoom RoomFromPoint(Point pt)
        {
            throw new NotImplementedException("there is no room in Microplate!");
        }

        public override IEnumerator GetEnumerator()
        {
            return _totalRegions.GetEnumerator();
        }

        public override ScanRegion this[int no]
        {
            get
            {
                return TotalRegions[no];
            }
        }

        // get character-based row id of the specified Region by 1-based Region no
        public string GetRowId(int no)
        {
            var row = GetRowNo(no);
            var r = (char)(row + 'A');
            return r.ToString();
        }

        // get 1-based column id of the specified Region by 1-based Region no
        public string GetColumnId(int no)
        {
            var col = GetColumnNo(no);	// 0-based
            return (col + 1).ToString();	// column id is 1-based
        }

        // get 0-based row # of the specified Region by 1-based Region no
        public int GetRowNo(int no)
        {
            return (no - 1) / ColumnCount;
        }

        // get 0-based column # of the specified Region by 1-based Region no
        public int GetColumnNo(int no)
        {
            return (no - 1) % ColumnCount;
        }

        public override void SortRegions(ArrayList scanRegions)
        {
            scanRegions.Sort(new PlateComparer(this, ScanPath));
        }

        protected override void ClearScanArea()
        {
            //_scanArea.Clear();
        }
        #endregion
    }


    public class PlateComparer : IComparer
    {
        private readonly Microplate _microplate;
        private readonly ScanPathType _scanpath;

        public PlateComparer(Microplate microplate, ScanPathType scanpath)
        {
            _microplate = microplate;
            _scanpath = scanpath;
        }

        public int Compare(object sr1, object sr2)
        {
            var w1 = (ScanRegion)sr1;
            var w2 = (ScanRegion)sr2;

            var row1 = _microplate.GetRowNo(w1.RegionId);
            var col1 = _microplate.GetColumnNo(w1.RegionId);
            var row2 = _microplate.GetRowNo(w2.RegionId);
            var col2 = _microplate.GetColumnNo(w2.RegionId);

            var comparison = row1 - row2;

            switch (_scanpath)
            {
                case ScanPathType.Serpentine:
                    if (comparison == 0)
                    {
                        if ((row1 / 2) * 2 == row1)			//check for even row number
                            comparison = col1 - col2;	//even go left to right
                        else
                            comparison = col2 - col1;	//odd go right to left
                    }
                    break;
                case ScanPathType.LeftToRight:
                    if (comparison == 0)
                    {
                        comparison = col1 - col2;		//even go left to right
                    }
                    break;
                case ScanPathType.RightToLeft:
                    if (comparison == 0)
                    {
                        comparison = col2 - col1;		//even go left to right
                    }
                    break;
                default:				//throw exception
                    throw new CyteException("Region.Comparer", "Region scanning order is not defined.");
            }
            return comparison;
        }
    }
}
