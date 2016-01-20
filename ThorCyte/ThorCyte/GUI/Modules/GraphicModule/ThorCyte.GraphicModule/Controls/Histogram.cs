using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls.RegionTools;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Controls
{
    public class Histogram : RegionCanvas
    {
        #region Constructor

        public Histogram()
        {
            Tools = new RegionTool[]
            {
                new  RegionToolPointer(),
                new  RegionToolRectangle()
            };
            CurrentTool = Tools[0];
        }

        #endregion

        #region Methods

        protected override void OnLoaded(object sender, RoutedEventArgs e)
        {
            base.OnLoaded(sender,e);
            if (_isLoading)
            {
                InitGraphics();
            }
            UnSelectAll();
            _isLoading = false;
        }

        protected void UpdateRegionPixels()
        {
            var regionList = new List<MaskRegion>();
            var ids = ROIManager.Instance.GetRegionIdList();
            foreach (var id in ids)
            {
                var region = ROIManager.Instance.GetRegion(id);
                if (region != null && region.GraphicId == Id)
                {
                    regionList.Add(region);
                }
            }
            if (regionList.Count > 0)
            {
                ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(Id, regionList, RegionUpdateType.Update));
            }
        }

        #endregion
    }
}
