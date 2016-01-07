using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{
    public abstract class Carrier : BindableBase
    {
        #region Fileds

        //protected ScanArea _scanArea;
        protected CarrierDef _carrierDef;
        protected Point _ref;
        protected Rect _activeRegionBounds = Rect.Empty;

        protected List<ScanRegion> _activeRegions = new List<ScanRegion>();
        protected List<ScanRegion> _totalRegions = new List<ScanRegion>();

        public static Point StageRef;
        public abstract IEnumerator GetEnumerator();
        public abstract ScanRegion this[int no] { get; }

        public abstract void SortRegions(ArrayList scanRegions);
        public abstract ScanRegion RegionFromPoint(Point pt);
        public abstract CarrierRoom RoomFromPoint(Point pt);
        protected abstract void ClearScanArea();

        #endregion

        #region Constructor

        protected Carrier(CarrierDef carrierDef)
        {
            _carrierDef = carrierDef;
            //_scanArea = new ScanArea(this);
        }

        #endregion

        #region Properties

        public Point ReferencePoint
        {
            get { return _ref; }
        }

        //public ScanArea ScanArea
        //{
        //    get { return _scanArea; }
        //}

        #region CarrierDef Properties

        public CarrierDef CarrierDef
        {
            get { return _carrierDef; }
        }

        #endregion

        public string Id
        {
            get { return _carrierDef.ID; }
        }

        public string Name
        {
            get { return _carrierDef.Name; }
        }

        public int Width
        {
            get { return _carrierDef.XWidth; }
        }

        public int Height
        {
            get { return _carrierDef.YHeight; }
        }

        public Size Size
        {
            get { return new Size(_carrierDef.XWidth, _carrierDef.YHeight); }
        }

        public int Thickness
        {
            get { return _carrierDef.Thickness; }
        }

        public string Description
        {
            get { return _carrierDef.Description; }
        }

        public int BaseToWellHeight
        {
            get { return _carrierDef.HeightBaseToWell; }
        }

        public int RetractionOffset
        {
            get { return _carrierDef.RetractionOffset; }
        }

        public bool Released
        {
            get { return _carrierDef.Released; }
        }
        #endregion

        #region Method

        // return scan region when only one region is active, otherwise return null
        public ScanRegion ActiveRegion
        {
            get
            {
                return _activeRegions.Count == 1 ? _activeRegions[0] : null;
            }
        }

        public IList<ScanRegion> TotalRegions
        {
            get
            {
                return _totalRegions;
            }
            set { _totalRegions = value.ToList(); }
        }

        public IList<ScanRegion> ActiveRegions
        {
            get { return _activeRegions; }
        }

        // return the first active region found, return null if no active region exists
        public ScanRegion FirstActiveRegion
        {
            get
            {
                return _activeRegions.Count > 0 ? _activeRegions[0] : null;
            }
        }

        public Rect ActiveRegionBounds
        {
            get { return _activeRegionBounds; }
        }

        public void AddSingleActiveRegion(ScanRegion sr)
        {
            _activeRegions.Clear();
            _activeRegions.Add(sr);
        }

        public void AddActiveRegion(ScanRegion sr)
        {
            _activeRegions.Add(sr);
        }

        public void RemoveActiveRegion(ScanRegion sr)
        {
            _activeRegions.Remove(sr);
        }

        public void ClearActiveRegions()
        {
            _activeRegions.Clear();
        }

        // convert device coord into slide coord
        public Point DeviceToCarrier(Point pt)
        {
            // for the old version that did not decouple the carrier coord from the StageRef
            if (_carrierDef.Type == CarrierType.Microplate && _ref != StageRef)
                return pt;

            pt.X -= _ref.X;
            pt.Y -= _ref.Y;
            return pt;
        }

        public Point CarrierToDevice(Point pt)
        {
            if (_carrierDef.Type == CarrierType.Microplate && _ref != StageRef)
                return pt;
            pt.X += _ref.X;
            pt.Y += _ref.Y;
            return pt;
        }

        public Rect CarrierToDevice(Rect rect)
        {
            if (_carrierDef.Type == CarrierType.Microplate && _ref != StageRef)
                return rect;

            rect.Offset(_ref.X, _ref.Y);
            return rect;
        }

        protected void Calibrate(Point ptRef)
        {
            _ref = ptRef;
        }

        protected void LoadScanRegions(List<ScanRegion> lstRegions)
        {
            _totalRegions = lstRegions;
        }

        #endregion
    }
}
