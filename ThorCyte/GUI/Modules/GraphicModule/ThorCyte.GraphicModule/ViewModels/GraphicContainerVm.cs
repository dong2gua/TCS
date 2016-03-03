using System;
using System.Collections.Generic;
using System.Globalization;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.ViewModels
{
    public class GraphicContainerVm : BindableBase
    {
        #region Fields

        private string _name;

        private bool _isEdit;

        private string _containerId;

        private bool _isShowProperty;

        private bool _isScatterProperVisible;

        private bool _isHistogramProperVisible;

        private bool _isDeleteEnabled;

        private bool _isNewGraphicEnabled;

        private bool _isPropertyEnabled;

        private readonly Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>> _graphicDictionary;

        private GraphicVmBase _selectedGraphic;

        private readonly ImpObservableCollection<GraphicVmBase> _graphicVmList;

        private static readonly IdManager _idManager = new IdManager();

        private readonly DelegateCommand _cancelEditCommand;

        #endregion

        #region Properties

        public bool IsEdit
        {
            get { return _isEdit; }
            set
            {
                if (_isEdit == value)
                {
                    return;
                }
                SetProperty(ref _isEdit, value);
            }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name == value)
                {
                    return;
                }
                if (GraphicModule.GraphicManagerVmInstance.IsValidateName(this, value))
                {
                    SetProperty(ref _name, value);
                }
                else
                {
                    UIHelper.OnCheckTabNameFailed(value);
                }
            }
        }

        public string ContainerId
        {
            get { return _containerId; }
            set
            {
                if (_containerId == value)
                {
                    return;
                }
                SetProperty(ref _containerId, value);
            }
        }

        public bool IsDeleteEnabled
        {
            get { return _isDeleteEnabled; }
            set
            {
                if (_isDeleteEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDeleteEnabled, value);
            }
        }

        public bool IsNewGraphicEnabled
        {
            get { return _isNewGraphicEnabled; }
            set
            {
                if (_isNewGraphicEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isNewGraphicEnabled, value);
            }
        }

        public bool IsPropertyEnabled
        {
            get { return _isPropertyEnabled; }
            set
            {
                if (_isPropertyEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isPropertyEnabled, value);
            }
        }

        public GraphicVmBase SelectedGraphic
        {
            get { return _selectedGraphic; }
            set
            {
                if (_selectedGraphic == value)
                {
                    return;
                }
                if (_selectedGraphic != null)
                {
                    _selectedGraphic.RegionToolType = ToolType.Pointer;
                }
                SetProperty(ref _selectedGraphic, value);
                IsDeleteEnabled = value != null;
                IsPropertyEnabled = value != null;
                SetPropertyVisible();
            }
        }

        public bool IsShowProperty
        {
            get { return _isShowProperty; }
            set
            {
                if (_isShowProperty == value)
                {
                    return;
                }
                SetProperty(ref _isShowProperty, value);
                SetPropertyVisible();
            }
        }

        public bool IsScatterProperVisible
        {
            get { return _isScatterProperVisible; }
            set
            {
                if (_isScatterProperVisible == value)
                {
                    return;
                }
                SetProperty(ref _isScatterProperVisible, value);
            }
        }

        public bool IsHistogramProperVisible
        {
            get { return _isHistogramProperVisible; }
            set
            {
                if (_isHistogramProperVisible == value)
                {
                    return;
                }
                SetProperty(ref _isHistogramProperVisible, value);
            }
        }

        public DelegateCommand CancelEditCommand
        {
            get { return _cancelEditCommand; }
        }

        public Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>> GraphicDictionary
        {
            get { return _graphicDictionary; }
        }

        public static IdManager IdManagerInstance
        {
            get { return _idManager; }
        }

        public ImpObservableCollection<GraphicVmBase> GraphicVmList
        {
            get { return _graphicVmList; }
        }

        #endregion

        #region Constructor

        public GraphicContainerVm(string name)
        {
            _graphicDictionary = new Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>>();
            _graphicVmList = new ImpObservableCollection<GraphicVmBase>();
            _name = name;
            _cancelEditCommand = new DelegateCommand(() => IsEdit = false);
            var components = ComponentDataManager.Instance.GetComponentNames();
            IsNewGraphicEnabled = components != null && components.Count > 0;
        }

        #endregion

        #region Methods

        public void SetPropertyVisible()
        {
            if (_selectedGraphic == null)
            {
                IsScatterProperVisible = false;
                IsHistogramProperVisible = false;
            }
            else
            {
                if (!_isShowProperty)
                {
                    IsScatterProperVisible = false;
                    IsHistogramProperVisible = false;
                    return;
                }
                if (_selectedGraphic is ScattergramVm)
                {
                    IsScatterProperVisible = true;
                    IsHistogramProperVisible = false;
                }
                else
                {
                    IsScatterProperVisible = false;
                    IsHistogramProperVisible = true;
                }
            }
        }

        public void OnDeleteGraphic(string id)
        {
            GraphicVmBase deleteVm = null;
            foreach (var vm in _graphicVmList)
            {
                if (vm.Id == id)
                {
                    deleteVm = vm;
                    break;
                }
            }
            if (deleteVm == null)
            {
                return;
            }
            var idInt = int.Parse(id);
            _idManager.RemoveId(idInt);
            var ids = ROIManager.Instance.GetRegionIdList();
            var regionList = new List<MaskRegion>();
            foreach (var regionId in ids)
            {
                var region = ROIManager.Instance.GetRegion(regionId);
                if (region != null && region.GraphicId == id)
                {
                    regionList.Add(region);
                }
            }
            _graphicDictionary.Remove(id);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(id, regionList, RegionUpdateType.Delete));
            _graphicVmList.Remove(deleteVm);

            if (_graphicVmList.Count == 0)
            {
                IsShowProperty = false;
                IsDeleteEnabled = false;
                IsPropertyEnabled = false;
            }
        }

        public void UpdateImage()
        {
            foreach (var graphicTuple in _graphicDictionary)
            {
                var scattergram = graphicTuple.Value.Item2 as ScattergramVm;
                if (scattergram != null )
                {
                    if (scattergram.XAxis.IsNormalize)
                    {
                        scattergram.XAxis.NormalizeToActiveWell();
                    }
                    if (scattergram.YAxis.IsNormalize)
                    {
                        scattergram.YAxis.NormalizeToActiveWell();
                    }
                }
                var histohgram = graphicTuple.Value.Item2 as HistogramVm;
                if (histohgram != null && histohgram.XAxis.IsNormalize)
                {
                    histohgram.XAxis.NormalizeToActiveWell();
                }
                graphicTuple.Value.Item2.UpdateEvents();
            }
        }

        public ScattergramVm CreateScattergram()
        {
            var id = _idManager.GetId().ToString(CultureInfo.InvariantCulture);
            var vm = new ScattergramVm{IsNormalizeXyEnabeld = true};
            vm.InitGraphParams(id);
            _graphicVmList.Add(vm);
            if (_graphicVmList.Count == 1)
            {
                SelectedGraphic = vm;
            }
            vm.XAxis.NormalizeToActiveWell();
            vm.YAxis.NormalizeToActiveWell();
            return vm;
        }

        public HistogramVm CreateHistogram()
        {
            var id = _idManager.GetId().ToString(CultureInfo.InvariantCulture);
            var vm = new HistogramVm { IsNormalizeXyEnabeld = true };
            vm.InitGraphParams(id);
            _graphicVmList.Add(vm);
            if (_graphicVmList.Count == 1)
            {
                SelectedGraphic = vm;
            }
            vm.XAxis.NormalizeToActiveWell();
            vm.SetIsNewEnabld();
            return vm;
        }

        public void Clear()
        {
            _graphicDictionary.Clear();
            _graphicVmList.Clear();
            _idManager.Clear();
        }

        #endregion
    }
}
