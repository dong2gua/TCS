using System;
using System.Diagnostics;
using System.Xml;
using ThorCyte.ProtocolModule.Models;
using ThorCyte.ProtocolModule.Utils;
using ThorCyte.ProtocolModule.Views.Modules;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class FilterModVm : ModuleBase
    {
        #region Properties and Fields

        public override bool Executable
        {
            get {
                return _selectedFilter != null;
            }
        }

        public override string CaptionString
        {
            get
            {
                var caption = _selectedFilter ?? string.Empty;
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

        public FilterModVm()
        {
            _passIndex = 1;
        }

        #endregion

        #region Methods

        public override void OnExecute()
        {
            try
            {
                if (_selectedFilter != null)
                {

                    //Todo: Add Implement logic here.

                    //Filter filter = Filter.GetFilter(_selectedFilter);
                    //BioImage img = filter.Convolve(InputImage);
                    //SetOutputImage(img);
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
            //foreach (Filter f in Filter.Filters)
            //{
            //    _filters.Add(f.ToString());
            //}

            if (_filters != null && _filters.Count > 0)
            {
                _selectedFilter = _filters[0];
            }
            OutputPort.DataType = PortDataType.GrayImage;
            OutputPort.ParentModule = this;
            InputPorts[0].DataType = PortDataType.GrayImage;
            InputPorts[0].ParentModule = this;
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("selectedfilter", SelectedFilter);
            writer.WriteAttributeString("passindex", PassIndex.ToString());
        }

        public override void OnDeserialize(XmlReader reader)
        {
            if (reader["selectedfilter"] != null)
            {
                SelectedFilter = reader["selectedfilter"];
            }
            if (reader["passindex"] != null)
            {
                PassIndex = XmlConvert.ToInt32(reader["passindex"]);
            }
        }

        #endregion
    }

}
