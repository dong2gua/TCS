using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.Carrier
{
    public class SlideComparer : IComparer<ScanRegion>
    {
        // sort by RoomNo, Y, then X
        public int Compare(ScanRegion sr1, ScanRegion sr2)
        {
            var rect1 = sr1.Bound;
            var rect2 = sr2.Bound;

            var dy = (int)(rect1.Top - rect2.Top);
            if (dy != 0) return dy;
            return (int)(rect1.Left - rect2.Left);
        }
    }

    public class NumberComparer : IComparer<ScanRegion>
    {
        // sort by region id
        public int Compare(ScanRegion sr1, ScanRegion sr2)
        {
            return sr1.RegionId - sr2.RegionId;
        }
    }

    public class Slide : Carrier
    {
        #region Fileds

        readonly ArrayList _rooms = new ArrayList();

        #endregion

        #region Properties

        public IList Rooms
        {
            get { return _rooms; }
        }

        public IList Regions
        {
            get { return _totalRegions; }
        }

        #endregion

        #region Constructor

        public Slide(CarrierDef carrierDef)
            : base(carrierDef)
        {
            CreateRooms();
            var sd = _carrierDef as SlideDef;
            if (sd == null) throw new CyteException("Carrier.Slide","Carrier define is null when constructing Slide object.");
            var topRight = new Point(StageRef.X + sd.MarginRight, StageRef.Y + sd.MarginTop);
            Calibrate(topRight);
        }

        #endregion


        #region Method
        private void CreateRooms()
        {
            foreach (RoomDef roomDef in ((SlideDef)_carrierDef).Rooms)
                AddRoomFrom(roomDef);

            if (_rooms.Count != 0) return;
            //One room at least.
            var rect = new Rect(0, 0, Width, Height);
            var room = new CarrierRoom(1, RegionShape.Rectangle, rect, RegionShape.Rectangle, rect);
            _rooms.Add(room);
        }

        public void AddRegion(ScanRegion rgn)
        {
            _totalRegions.Add(rgn);
        }

        public void RemoveRegion(ScanRegion rgn)
        {
            _totalRegions.Remove(rgn);
        }

        public void ClearRegions()
        {
            _totalRegions.Clear();
        }

        public override IEnumerator GetEnumerator()
        {
            return _totalRegions.GetEnumerator();
        }

        public override ScanRegion this[int no]
        {
            get
            { return _totalRegions.FirstOrDefault(rgn => rgn.RegionId == no); }
        }

        public override void SortRegions(ArrayList scanRegions)
        {
            _totalRegions.Sort(new SlideComparer());	// sort by RoomNo, Y, X

            _totalRegions.Sort(new NumberComparer());	// sort by well-no

        }

        public override ScanRegion RegionFromPoint(Point pt)
        {
            //return _totalRegions.FirstOrDefault(rgn => rgn.IsVisible(pt)); need implement!
            return null;
        }

        public void AddRoomFrom(RoomDef roomDef)
        {
            var no = roomDef.Number;
            var rect = roomDef.rect;
            var shape = roomDef.Shape;
            var scanRect = roomDef.rect;
            var scanShape = roomDef.Shape;

            var room = new CarrierRoom(no, shape, rect, scanShape, scanRect);
            _rooms.Add(room);
        }

        public override CarrierRoom RoomFromPoint(Point pt)
        {
            return _rooms.Cast<CarrierRoom>().FirstOrDefault(room => room.Rect.Contains(pt));
        }



        protected override void ClearScanArea()
        {
            Regions.Clear();

            _activeRegionBounds = new Rect(0, 0, 0, 0);

        }
        #endregion
    }
}
