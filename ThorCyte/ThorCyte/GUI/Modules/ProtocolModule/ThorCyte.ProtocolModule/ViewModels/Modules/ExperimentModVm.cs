using System.Collections.ObjectModel;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.ViewModels.ModulesBase;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ExperimentModVm : ModuleVmBase
    {
        private IData _dataManager;



        public override string CaptionString
        {
            get { return string.Format("Scan({0})",SelectedScanId);}
        }

        public override void Initialize()
        {
            base.Initialize();

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
            _dataManager = ServiceLocator.Current.GetInstance<IData>();
            ScanIdList.Clear();
            ScanIdList.Add(Macro.CurrentScanId);
        }


        protected void AnalyzeImage()
        {
            var img = 
            Macro.CurrentImage = img;
            OutputPort.Image = img;
            base.Execute();


            //OnImageAnalyzed(img);
        }


    }
}
