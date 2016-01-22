using System;
using System.Windows;
using ImageProcess.DataType;
using ThorCyte.Infrastructure.Types;

namespace ComponentDataService.Types
{
   
    public class BioEvent : ICloneable
    {
        
        #region Fields
        private readonly float[] _buffer;// buffer to contain feature data
        #endregion

        #region Properties
        public int FeatureCount { get; private set; }

        public int Id	// the first feature must be Id
        {
            get { return (int)_buffer[0]; }
            set { _buffer[0] = value; }
        }

        public float this[int fi]
        {
            get { return _buffer[fi]; }
            set { _buffer[fi] = value; }
        }

        public float[] Buffer
        {
            get { return _buffer; }
        }

        public Rect BoundRect { get; set; }

        public Blob DataBlob { get; set; }
        public Blob ContourBlob { get; set; }
        public Blob BackgroundBlob { get; set; }
        public Blob PeripheralBlob { get; set; }
        public RegionColorIndex ColorIndex { get; set; }

        #endregion


        #region Constructors
        public BioEvent(int featureCnt)
        {
            BoundRect = Rect.Empty;
            _buffer = new float[featureCnt];
            FeatureCount = featureCnt;
            ColorIndex = RegionColorIndex.Black;
        }

        public BioEvent() : this(0)
        {

        }

        #endregion


        #region Methods

        private void CopyTo(BioEvent dst)
        {
            _buffer.CopyTo(dst._buffer, 0);
        }

        public BioEvent Clone()
        {
            var bEvent = new BioEvent(FeatureCount) { Id = Id };
            CopyTo(bEvent);
            return bEvent;
        }
     
        object ICloneable.Clone()
        {
            return Clone();
        }

        #endregion


    }
}
