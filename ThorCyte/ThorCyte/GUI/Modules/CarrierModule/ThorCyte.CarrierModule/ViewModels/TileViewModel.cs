using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.CarrierModule.ViewModels
{
    public class TileItem
    {
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public int FieldId { get; set; }

        public TileItem()
        {
            Left = 0;
            Top = 0;
            Width = 0;
            Height = 0;
            FieldId = 0;
        }

        public Rect TileRect
        {
            get
            {
                return new Rect(Left, Top, Width, Height);
            }
            set
            {
                Left = (int)value.Left;
                Top = (int)value.Top;
                Width = (int)value.Width;
                Height = (int)value.Height;
            }
        }

    }

    public class TileViewModel : BindableBase
    {
        private ObservableCollection<TileItem> _tilesShowInCanvas;
        public ICommand CmdTileTrigger { get; private set; }

        private double _viewSizeMax;
        private int _initialViewSize = 600;
        private double _pxFactor = 1.0; // 1 pixel equals how many length unit?
        private double _p0x;      // ScanRegion real left position
        private double _p0y;      // ScanRegion real top position

        private int _viewHeight; //in pixel
        private int _viewWidth; //in pixel

        private ScanRegion _inRegion = null;


        private static IEventAggregator EventAggregator
        {
            get { return ServiceLocator.Current.GetInstance<IEventAggregator>(); }
        }

        public ObservableCollection<TileItem> TilesShowInCanvas
        {
            get
            {
                return _tilesShowInCanvas;
            }
            set { SetProperty(ref _tilesShowInCanvas, value); }
        }

        public double ViewSizeMax
        {
            get
            {
                return _viewSizeMax;
            }
            set
            {
                SetProperty(ref _viewSizeMax, value);
                LoadTiles(_inRegion);
            }
        }

        public int InitialViewSize
        {
            get
            {
                return _initialViewSize;
            }
            set { SetProperty(ref _initialViewSize, value); }
        }

        private string _regionid;

        public string RegionID
        {
            get
            {
                return _regionid;
            }
            private set { SetProperty(ref _regionid, value); }
        }


        public int ViewHeight
        {
            get
            {
                return _viewHeight;
            }
            set { SetProperty(ref _viewHeight, value); }
        }

        public int ViewWidth
        {
            get
            {
                return _viewWidth;
            }
            set { SetProperty(ref _viewWidth, value); }
        }



        public TileViewModel()
        {
            //subscribe select region event.
            var selectRegionEvt = EventAggregator.GetEvent<SelectRegions>();
            selectRegionEvt.Subscribe(OnSelectRegionChanged);

            //delegate button click command
            CmdTileTrigger = new DelegateCommand<object>(this.OnTileSelect);


            //Define button infomations to show on UI
            _tilesShowInCanvas = new ObservableCollection<TileItem>();

            //SetEmptyContent();
        }

        public void SetEmptyContent()
        {
            TilesShowInCanvas.Clear();
            _inRegion = null;
            ViewHeight = 0;
            ViewWidth = 0;
            RegionID = string.Empty;
        }


        /// <summary>
        /// Response the Tile Button Click 
        /// </summary>
        /// <param name="oItem">Tile item.</param>
        private void OnTileSelect(object oItem)
        {
            var tItem = oItem as TileItem;
            if (tItem == null) return;

            EventAggregator.GetEvent<SelectRegionTileEvent>().Publish(new RegionTile()
            {
                TileId = _inRegion.ScanFieldList[tItem.FieldId - 1].ScanFieldId,
                RegionId = _inRegion.RegionId
            });

            Debug.WriteLine("Tile id = " + tItem.FieldId + " Published!");
        }


        /// <summary>
        /// Load All Tiles from the ScanRegion.
        /// </summary>
        /// <param name="sr">ScanRegion</param>
        private void LoadTiles(ScanRegion sr)
        {
            if (sr == null) return;

            _inRegion = sr;
            RegionID = _inRegion != null ? "Region ID: " + _inRegion.RegionId : string.Empty;
            if (_viewHeight == 0)
                _viewSizeMax = InitialViewSize;

            CalcPxFactor(sr.Bound);
            //ResetViewSize
            ViewHeight = GetLengthAsPixel(sr.Bound.Height);
            ViewWidth = GetLengthAsPixel(sr.Bound.Width);

            TilesShowInCanvas.Clear();
            foreach (var scanfield in sr.ScanFieldList)
            {
                //if (!IsRectInside(sr.Bound, scanfield.SFRect))
                //{
                //    Debug.WriteLine("ScanField(" + scanfield.ScanFieldId + ") is outside of ScanRegion! please check!");
                //    continue;
                //}

                TilesShowInCanvas.Add(Convert(scanfield));
            }
        }

        /// <summary>
        /// Convert scanfield
        /// </summary>
        /// <param name="sf"></param>
        /// <returns></returns>
        private TileItem Convert(Scanfield sf)
        {
            var ti = new TileItem() { FieldId = sf.ScanFieldId };
            var p = GetPositionAsPixel(sf.SFRect.Left, sf.SFRect.Top);

            var rT = new Rect((int)p.X, (int)p.Y, GetLengthAsPixel(sf.SFRect.Width), GetLengthAsPixel(sf.SFRect.Height));

            //ti.Left = (int)p.X;
            //ti.Top = (int) p.Y;
            //ti.Width = GetLengthAsPixel(sf.SFRect.Width);
            //ti.Height = GetLengthAsPixel(sf.SFRect.Height);

            //Trasform to Right/Top coordinate.
            ti.TileRect = CoordinateTransform(rT);

            return ti;
        }

        private void OnSelectRegionChanged(List<int> args)
        {
            try
            {
                if (args.Count != 1)
                {
                    //Clear Canvas items and set size to 0,0
                    SetEmptyContent();
                    return;
                }

                var sr = CarrierModule.Instance.CurrentScanInfo.ScanRegionList[args[0]];
                LoadTiles(sr);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error Occured in OnSelectRegionChanged! " + ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Calculate pixel factor according to ScanRegion Bound
        /// </summary>
        /// <param name="sBoudRect">Scan Region bound rectangle</param>
        private void CalcPxFactor(Rect sBoudRect)
        {
            try
            {
                var l = sBoudRect.Width >= sBoudRect.Height ? sBoudRect.Width : sBoudRect.Height;

                _pxFactor = l / ViewSizeMax;

                Debug.WriteLine("Calculate pxFactor = " + _pxFactor);

                _p0x = sBoudRect.Left;
                _p0y = sBoudRect.Top;

                Debug.WriteLine("Calculate p0x = " + _p0x + "; p0y = " + _p0y);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error Occured in CalcPxFactor! System will use default factor 1 ! " + ex.Message);
                throw;
            }
        }

        private int GetLengthAsPixel(double realLength)
        {
            return (int)(realLength / _pxFactor);
        }

        private Point GetPositionAsPixel(double realpx, double realpy)
        {
            var X = GetLengthAsPixel(realpx - _p0x);
            var Y = GetLengthAsPixel(realpy - _p0y);
            return new Point(X, Y);
        }


        /// <summary>
        /// determine if rect son is inside rect parent.
        /// </summary>
        /// <param name="parent">Parent rectangle</param>
        /// <param name="son">Son rectangle</param>
        /// <returns>Inside --true, Not Inside --false</returns>
        private bool IsRectInside(Rect parent, Rect son)
        {
            var tolerance = 2;
            
            if (son.Left+tolerance < parent.Left)
            {
                Debug.WriteLine("Rect son left = " + son.Left + " < parent left = " + parent.Left);
                return false;
            }

            if (son.Right > parent.Right+tolerance)
            {
                Debug.WriteLine("Rect son left = " + son.Left + " < parent left = " + parent.Left);
                return false;
            }

            if (son.Top+tolerance < parent.Top)
            {
                Debug.WriteLine("Rect son top = " + son.Top + " < parent top = " + parent.Top);
                return false;
            }

            if (son.Bottom > parent.Bottom + tolerance)
            {
                Debug.WriteLine("Rect son bottom = " + son.Bottom + " > parent bottom = " + parent.Bottom);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Perform Coordinate Transform.
        /// </summary>
        /// <param name="o">original rectangle</param>
        /// <returns>transformed rectangle</returns>
        private Rect CoordinateTransform(Rect o)
        {
            var r = new Rect();

            //r.X = ViewWidth - o.Left;
            r.X = ViewWidth - o.Left - o.Width;
            r.Y = o.Top;
            r.Width = o.Width;
            r.Height = o.Height;
            return r;
        }

    }
}
