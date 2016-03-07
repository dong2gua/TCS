using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using ImageProcess;
using ImageProcess.DataType;
using ThorCyte.Infrastructure.Interfaces;

namespace ComponentDataService.Types
{
    internal class PhantomComponent : BioComponent
    {
       

        #region Fields

        private readonly Dictionary<int, List<Blob>> _blobDictionary = new Dictionary<int, List<Blob>>(DefaultBlobsCount);
        private const int MaxPerimeter = 10;
        #endregion    
 
        #region Constructors
        public PhantomComponent(IExperiment experiment, string name,int scanId) : base(experiment, name, scanId)
        {           
        }

        public PhantomComponent(IExperiment experiment, string name) : base(experiment, name, 1)
        {           
        }
        #endregion

        #region Methods
        #region Internal
        internal List<Blob> CreatePhantomBlobs(int wellId, int tileId, PhantomDefine define)
        {
            int key = GetBlobKey(wellId, tileId);
            List<Blob> blobs = define.Pattern == PhantomDefine.PhantomPattern.Lattice
                ? CreateLatticePhantomBlobs(define)
                : CreateRandomPhantomBlobs(define);
            _blobDictionary[key] = blobs;
            return blobs;
        }

        internal override List<Blob> GetTileBlobs(int wellId, int tileId, BlobType type)
        {
            int key = GetBlobKey(wellId, tileId);
            if (_blobDictionary.ContainsKey(key) == false)
            {
                _blobDictionary[key] = EmptyBlobs.ToList();
            }
            return _blobDictionary[key];
        }

        internal override void SaveTileBlobs(string baseFolder)
        {

        }

        internal override IList<BioEvent> CreateEvents(int wellId, int tileId,
            IDictionary<string, ImageData> imageDict, BlobDefine define)
        {
            define = ToVaildBlobDefine(define);
            int key = GetBlobKey(wellId, tileId);
            List<Blob> contours = _blobDictionary[key];
            List<BioEvent> evs = new List<BioEvent>(contours.Count);
            if (EventsDict.ContainsKey(wellId) == false)
            {
                EventsDict[wellId] = new List<BioEvent>();
            }
            List<BioEvent> stored = EventsDict[wellId];
            int id = stored.Count + 1;//1 base
            foreach (Blob contour in contours)
            {
                contour.Id = id;
                BioEvent ev = CreateEvent(contour, contour, define, imageDict, wellId, tileId);
                if (ev != null)
                    evs.Add(ev);
            }
            stored.AddRange(evs);
            ResetEventCountDict();
            return evs;
        }
        #endregion

        #region Private
        private List<Blob> CreateLatticePhantomBlobs(PhantomDefine define)	// in micron
        {
            double radius = define.Radius;
            double distance = define.Distance;
            int scanId = Experiment.GetCurrentScanId();
            ScanInfo info = Experiment.GetScanInfo(scanId);
            if (info == null) return EmptyBlobs.ToList();
            int width = info.TileWidth - 2;
            double pixelWidth = info.XPixcelSize;
            double pixelHeight = info.YPixcelSize;
            int xradius = (int)(radius / pixelWidth);
            int xdist = (int)(distance / pixelWidth);	// in pixel
            if (xradius == 0 || xdist == 0) // jcl-5504
                return EmptyBlobs.ToList();
            int cols = width / xdist;		// phantom column count 
            xdist = width / cols;	// adjusted distance
            int xstart = xradius + 1;
            int height = info.TiledHeight - 2;
            int yradius = (int)(radius / pixelHeight);
            int ydist = (int)(distance / pixelHeight);	// in pixel
            if (yradius == 0 || ydist == 0)
                return EmptyBlobs.ToList();
            int rows = height / ydist;		// phantom column count 
            ydist = height / rows;	// adjusted distance
            int ystart = yradius + 1;
            Point ptCenter = default(Point);
            List<Blob> blobs = new List<Blob>();
            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    ptCenter.X = xdist * col + xstart;
                    ptCenter.Y = ydist * row + ystart;
                    Blob phantom = Blob.CreatePhantomBlobs(ptCenter, radius, new Size(ImageWidth,ImageHeight),
                        new Size(pixelWidth, pixelHeight)); // jcl-5508
                    if (phantom != null)
                        blobs.Add(phantom);
                }
            }
            return blobs;
        }

        private List<Blob> CreateRandomPhantomBlobs(PhantomDefine define)
        {
            double radius = define.Radius;
            double distance = define.Distance;
            int count = define.Count;
            int scanId = Experiment.GetCurrentScanId();
            ScanInfo info = Experiment.GetScanInfo(scanId);
            if (info == null) return EmptyBlobs.ToList();
            int imageWidth = info.TileWidth;
            int imageHeight = info.TiledHeight;
            double pixelWidth = info.XPixcelSize;
            double pixelHeight = info.YPixcelSize;
            const int maxTries = 100;
            Random random = new Random();

            //make sure blob fits in scan area
            double dWidth = imageWidth * pixelWidth; // micron
            double dHeight = imageHeight * pixelHeight;

            int nMaxRadius = (int)(dWidth < dHeight ? dWidth : dHeight);
            nMaxRadius = (nMaxRadius / 2) - MaxPerimeter; // micron

            if (radius > nMaxRadius)
                radius = nMaxRadius;
            int offsetX = (int)(radius / pixelWidth + maxTries + 1); // jcl-40xphantom, jcl-crashes, pixel
            int offsetY = (int)(radius / pixelHeight + maxTries + 1);

            double nMinDistanceSquared = distance * distance; // micron sq, using Distance as min Distance?

            List<Blob> blobs = new List<Blob>();

            for (int i = 0; i < count; i++)
            {
                // find a random center that is dfMinDistance away from all other phantom blobs,
                // try MAX_TRIES times only
                bool bFoundSpaceForBlob = false;
                Point ptBlobCenter = new Point();
                Blob phantom = null;

                for (int iTry = 0; iTry < maxTries && !bFoundSpaceForBlob; iTry++)
                {
                    // find a random center for the new blob
                    ptBlobCenter.X = random.Next() % (imageWidth - offsetX * 2); // jcl-40xphantom, jcl-crashes
                    ptBlobCenter.Y = random.Next() % (imageHeight - offsetY * 2); // jcl-40xphantom
                    ptBlobCenter.X += offsetX;
                    ptBlobCenter.Y += offsetY;

                    phantom = Blob.CreatePhantomBlobs(ptBlobCenter, radius, new Size(imageWidth, imageHeight),
                        new Size(pixelWidth, pixelHeight)); // jcl-5508
                    if (phantom == null) continue;	// phantom is out of bounds
                    bFoundSpaceForBlob = true;
                    //Make sure center is MinDistance away from other phantom blobs
                    foreach (Blob blobPhantom in blobs)
                    {
                        Int32Rect rect = blobPhantom.Bound;
                        Point ptCenter = new Point(rect.X + rect.Width / 2, rect.Y + rect.Height / 2);

                        //distance is sqrt( xDiff^2 + yDiff^2)
                        int xDiff = (int)((ptCenter.X - ptBlobCenter.X) * pixelWidth);	// in micron
                        int yDiff = (int)((ptCenter.Y - ptBlobCenter.Y) * pixelHeight);
                        if ((xDiff * xDiff + yDiff * yDiff) < nMinDistanceSquared)
                        {
                            bFoundSpaceForBlob = false;
                            phantom = null;
                            break;
                        }
                    }
                }

                if (phantom != null)
                    blobs.Add(phantom);
            }

            return blobs;
        }

        private BlobDefine ToVaildBlobDefine(BlobDefine define)
        {
            define.IsDynamicBackground = false;
            define.IsPeripheral = false;
            return define;
        }
        #endregion

        #endregion


     
    }
}
