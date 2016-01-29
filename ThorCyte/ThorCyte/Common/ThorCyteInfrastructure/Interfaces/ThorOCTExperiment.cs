using System;
using System.Windows.Media.Imaging;


namespace ThorCyte.Infrastructure.Interfaces
{
    public class ThorOCTExperiment : IExperiment
    {
        public void Load(string experimentPath)
        {
            throw new NotImplementedException();
        }

        public ExperimentInfo GetExperimentInfo()
        {
            throw new NotImplementedException();
        }

        public int GetScanCount()
        {
            throw new NotImplementedException();
        }

        public ScanInfo GetScanInfo(int scanId)
        {
            throw new NotImplementedException();
        }

        public string GetCarrierType()
        {
            throw new NotImplementedException();
        }

        public BitmapSource GetCameraView()
        {
            throw new NotImplementedException();
        }

        public int GetCurrentScanId()
        {
            throw new NotImplementedException();
        }
    }
}
