using System.Collections.ObjectModel;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ExperimentModVm : ModuleBase
    {
        public override string CaptionString
        {
            get { return string.Format("Scan({0})",SelectedScanId);}
        }

        public override void Initialize()
        {
            base.Initialize();

            HasImage = false;
            View = new ExperimentModule();
            ModType = ModuleType.SmtSystemCategory;
            OutputPort.DataType = PortDataType.MultiChannelImage;
            OutputPort.ParentModule = this;

            if (ScanIdList.Count > 0)
            {
                SelectedScanId = ScanIdList[0];
            }
        }

        public ObservableCollection<int> ScanIdList { get; set; }

        private int _selectedScanId;

        public int SelectedScanId
        {
            get { return _selectedScanId; }
            set { SetProperty(ref _selectedScanId, value); }
        } 

        public ExperimentModVm()
        {
            ScanIdList = new ImpObservableCollection<int> {Macro.CurrentScanId};
        }

        public void AnalyzeImage(int regionId,int tileId)
        {
            OutputPort.ScanId = SelectedScanId;
            OutputPort.RegionId = regionId;
            OutputPort.TileId = tileId; 
            base.Execute();
        }


    }
}
