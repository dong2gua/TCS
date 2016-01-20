using System;
using System.Collections.Generic;
using System.Globalization;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Events;
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

        private readonly Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>> _graphicDictionary;

        private GraphicVmBase _selectedGraphic;

        private readonly ImpObservableCollection<GraphicVmBase> _graphicVmList;

        private static readonly IdManager _idManager = new IdManager();

        private readonly DelegateCommand _deleteGraphicCmd;

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
                SetProperty(ref _name, value);
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

        public GraphicVmBase SelectedGraphic
        {
            get { return _selectedGraphic; }
            set
            {
                if (_selectedGraphic == value)
                {
                    return;
                }
                SetProperty(ref _selectedGraphic, value);
                IsDeleteEnabled = value != null;
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

        public DelegateCommand DeleteGraphicCmd
        {
            get { return _deleteGraphicCmd; }
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
            _deleteGraphicCmd = new DelegateCommand(OnDeleteGraphic);
            _cancelEditCommand = new DelegateCommand(()=>IsEdit = false);
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

        private void OnDeleteGraphic()
        {
            if (_selectedGraphic == null)
            {
                return;
            }
            var idInt = int.Parse(_selectedGraphic.Id);
            _idManager.RemoveId(idInt);
            var ids = ROIManager.Instance.GetRegionIdList();
            var regionList = new List<MaskRegion>();
            foreach (var regionId in ids)
            {
                var region = ROIManager.Instance.GetRegion(regionId);
                if (region != null && region.GraphicId == _selectedGraphic.Id)
                {
                    regionList.Add(region);
                }
            }
            _graphicDictionary.Remove(_selectedGraphic.Id);
            ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<RegionUpdateEvent>().Publish(new RegionUpdateArgs(_selectedGraphic.Id, regionList, RegionUpdateType.Delete));
            _graphicVmList.Remove(_selectedGraphic);
        }

        public void UpdateImage()
        {
            foreach (var graphicTuple in _graphicDictionary)
            {
                graphicTuple.Value.Item2.UpdateEvents();
            }
        }

        public void CreateScattergram()
        {
            var id = _idManager.GetId().ToString(CultureInfo.InvariantCulture);
            var vm = new ScattergramVm();
            vm.InitGraphParams(id);
            _graphicVmList.Add(vm);
            GraphicModule.GraphicManagerVmInstance.UpdateRegionList();
        }

        public void CreateHistogram()
        {
            var id = _idManager.GetId().ToString(CultureInfo.InvariantCulture);
            var vm = new HistogramVm();
            vm.InitGraphParams(id);
            _graphicVmList.Add(vm);
        }

        #endregion
    }
}
