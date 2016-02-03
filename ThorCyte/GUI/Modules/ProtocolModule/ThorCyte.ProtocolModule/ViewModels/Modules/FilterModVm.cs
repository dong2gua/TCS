using System;
using System.Diagnostics;
using System.Xml;
using ImageProcess;
using ThorCyte.Infrastructure.Exceptions;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class FilterModVm : ModuleBase
    {
        #region Properties and Fields
        private ImageData _img;

        public override bool Executable
        {
            get {
                return _selectedFilter != null && _selectedKSize != null;
            }
        }

        public override string CaptionString
        {
            get
            {
                var caption = _selectedFilter + "\n" + _selectedKSize;
                return string.Format("{0}({1})", caption, _passIndex);
            }
        }

        private string _selectedFilter;

        public string SelectedFilter
        {
            get { return _selectedFilter; }
            set
            {
                if (_selectedFilter == value)
                {
                    return;
                }
                SetProperty(ref _selectedFilter, value);
                OnPropertyChanged("CaptionString");

                GetKSizes(_selectedFilter);

            }
        }


        private string _selectedKSize;

        public string SelectedKSize
        {
            get { return _selectedKSize; }
            set
            {
                if (_selectedKSize == value)
                {
                    return;
                }
                SetProperty(ref _selectedKSize, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private int _passIndex;

        public int PassIndex
        {
            get { return _passIndex; }
            set
            {
                if (_passIndex == value)
                {
                    return;
                }
                _passIndex = value;
                SetProperty(ref _passIndex, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private ImpObservableCollection<string> _filters = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> Filters
        {
            get { return _filters; }
            set { _filters = value; }
        }

        private ImpObservableCollection<string> _ksizes = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> KSizes
        {
            get { return _ksizes; }
            set { _ksizes = value; }
        }

        public FilterModVm()
        {
            _passIndex = 1;
            GetAllFilters();
        }

        #endregion

        #region Methods

        public void GetAllFilters()
        {
            Filters.Clear();
            foreach (var name in Enum.GetNames(typeof(FilterType)))
            {
                Filters.Add(name);
            }
        }

        public void GetKSizes(string seleft)
        {
            FilterType ft;

            Enum.TryParse(seleft, true,out ft); 
            
            KSizes.Clear();
            foreach (var i in ImageData.GetSupportedKernelSize(ft))
            {
                KSizes.Add(string.Format("{0} x {0}",i));
            }

            if (_ksizes != null && _ksizes.Count > 0)
            {
                SelectedKSize = _ksizes[0];
            }
        }


        public override void OnExecute()
        {
            try
            {
                if (_selectedFilter != null && _selectedKSize != null)
                {
                    _img = InputImage;
                    FilterType ft;
                    Enum.TryParse(_selectedFilter, true, out ft);
                    var masksize = Convert.ToInt32(_selectedKSize.Substring(0, 1));

                    var processedImg = _img.CommonFilter(ft, masksize,_passIndex);

                    _img.Dispose();

                    if (processedImg == null)
                    {
                        throw new CyteException("FilterModVm", "Invaild execution image is null");
                    }
                    SetOutputImage(processedImg);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Filter Module error: " + ex.Message);
                throw;
            }
        }


        public override void Initialize()
        {
            View = new FilterModule();
            ModType = ModuleType.SmtFilterModule;
            Name = GlobalConst.FilterModuleName;
            HasImage = true;

            
            if (_filters != null && _filters.Count > 0)
            {
                SelectedFilter = _filters[0];
            }

            if (_ksizes != null && _ksizes.Count > 0)
            {
                SelectedKSize = _ksizes[0];
            }

            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("selectedfilter", SelectedFilter);
            writer.WriteAttributeString("selectedkernelsize", SelectedKSize);
            writer.WriteAttributeString("passindex", PassIndex.ToString());
        }

        public override void OnDeserialize(XmlReader reader)
        {
            if (reader["selectedfilter"] != null)
            {
                SelectedFilter = reader["selectedfilter"];
            }

            if (reader["selectedkernelsize"] != null)
            {
                SelectedKSize = reader["selectedkernelsize"];
            }

            if (reader["passindex"] != null)
            {
                PassIndex = XmlConvert.ToInt32(reader["passindex"]);
            }
        }

        #endregion
    }

}
