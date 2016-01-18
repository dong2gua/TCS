using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using ComponentDataService;
using ComponentDataService.Types;
using ThorCyte.Infrastructure.Types;

namespace ROIService.Region
{
    public abstract class MaskRegion
    {
        
        #region Enum
        public enum Position { None, Upper, Bottom, Left, Right, Center };
        #endregion
      
        #region Property

        public int Id { get; private set; }
        public FeatureType FeatureTypeNumeratorX { get; set; }
        public FeatureType FeatureTypeDenominatorX { get; set; }
        public FeatureType FeatureTypeNumeratorY { get; set; }
        public FeatureType FeatureTypeDenominatorY { get; set; }
        public FeatureType FeatureTypeZ { get; set; }
        public Position LabelPosition { get; set; }
        public Color Color { get; set; }
        public string ChannelNumeratorX { get; set; }
        public string ChannelDenominatorX { get; set; }
        public string ChannelNumeratorY { get; set; }
        public string ChannelDenominatorY { get; set; }
        public string ChannelZ;
        public RegionShape Shape { get; protected set; }      
        public string ComponentName { get; set; }
        public string LeftParent { get; set; }//master parent
        public string RightParent { get; set; }//slave parent
        public List<string> Children { get; set; }
        public OperationType Operation { get; set; }
        public string GraphicId { get; set; }
        public int IndexNumX { get; private set; }
        public int IndexDenoX { get; private set; }
        public int IndexNumY { get; private set; }
        public int IndexDenoY { get; private set; }
        public bool IsLogscaleX { get; set; }
        public bool IsLogScaleY { get; set; }

        #endregion

        #region Abstract and virtual method
        public abstract bool Contains(Point pt);

     
        #endregion

        #region Constructor

        protected MaskRegion(int id)
        {
            Id = id;
            InitPropertyMaybeNotUsed();
            Children = new List<string>();
        }


        #endregion
       
        private void InitPropertyMaybeNotUsed()
        {
            FeatureTypeZ = FeatureType.None;            
            FeatureTypeDenominatorX = FeatureType.None;
            FeatureTypeNumeratorY = FeatureType.None;
            FeatureTypeDenominatorY = FeatureType.None;
            LeftParent = string.Empty;
            RightParent = string.Empty;
        }
 
        public void Calculate()
        {          
            CalculateFeatureIndex();  
        }

        private void CalculateFeatureIndex()
        {
            IndexNumX = ComponentDataManager.Instance.GetFeatureIndex(ComponentName, FeatureTypeNumeratorX,
                ChannelNumeratorX);
            IndexDenoX = ComponentDataManager.Instance.GetFeatureIndex(ComponentName, FeatureTypeDenominatorX,
                ChannelDenominatorX);
            IndexNumY = ComponentDataManager.Instance.GetFeatureIndex(ComponentName, FeatureTypeNumeratorY,
                ChannelNumeratorY);
            IndexDenoY = ComponentDataManager.Instance.GetFeatureIndex(ComponentName, FeatureTypeDenominatorY,
                ChannelDenominatorY);
        }

        protected Point ToInnerPoint(Point pt)
        {
            var x = IsLogscaleX ? Math.Log10(pt.X) : pt.X;
            var y = IsLogScaleY ? Math.Log10(pt.Y) : pt.Y;
            return new Point(x, y);

        }


    }
}
