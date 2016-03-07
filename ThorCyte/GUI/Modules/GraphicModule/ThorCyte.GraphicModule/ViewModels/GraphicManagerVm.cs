using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml;
using System.Xml.Linq;
using ComponentDataService;
using Microsoft.Practices.ServiceLocation;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ROIService;
using ROIService.Region;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Controls.Graphics;
using ThorCyte.GraphicModule.Events;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Models;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.GraphicModule.Views;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte.GraphicModule.ViewModels
{
    public class GraphicManagerVm : BindableBase
    {
        #region Fields

        private readonly List<GraphicVmBase> _tempGraphicvmList = new List<GraphicVmBase>();

        private static ROIManager _roiInstance;

        private readonly IdManager _tabIdManager;

        private GraphicContainerVm _selectedContainer;

        private IList<int> _activeWellNos;

        private bool _isControlEnabled;

        private bool _isDeleteTabEnabled;

        private readonly DelegateCommand _addTabCmd;

        private readonly DelegateCommand _deleteTabCmd;

        private readonly ImpObservableCollection<GraphicContainerVm> _graphicContainerVms;

        #endregion

        #region Properties

        public bool IsControlEnabled
        {
            get { return _isControlEnabled; }
            set
            {
                if (_isControlEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isControlEnabled, value);
            }
        }

        public bool IsDeleteTabEnabled
        {
            get { return _isDeleteTabEnabled; }
            set
            {
                if (_isDeleteTabEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDeleteTabEnabled, value);
            }
        }

        public DelegateCommand AddTabCmd
        {
            get { return _addTabCmd; }
        }

        public DelegateCommand DeleteTabCmd
        {
            get { return _deleteTabCmd; }
        }

        public GraphicContainerVm SelectedContainer
        {
            get { return _selectedContainer; }
            set
            {
                if (_selectedContainer == value)
                {
                    return;
                }
                SetProperty(ref _selectedContainer, value);
                IsControlEnabled = _selectedContainer != null;
                IsDeleteTabEnabled = _selectedContainer != null && _graphicContainerVms.Count > 1;
            }
        }

        public static bool IsLoadGateEnd { get; set; }

        public ImpObservableCollection<GraphicContainerVm> GraphicContainerVms
        {
            get { return _graphicContainerVms; }
        }

        public IList<int> ActiveWellNos
        {
            get { return _activeWellNos; }
        }

        #endregion

        #region Constructor

        public GraphicManagerVm()
        {
            _graphicContainerVms = new ImpObservableCollection<GraphicContainerVm>();
            _roiInstance = ROIManager.Instance;
            _tabIdManager = new IdManager();
            _addTabCmd = new DelegateCommand(OnAddTab);
            _deleteTabCmd = new DelegateCommand(OnDeleteTab);
            var eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>();
            eventAggregator.GetEvent<RegionUpdateEvent>().Subscribe(UpdateRegion);
            eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(LoadXml);
            eventAggregator.GetEvent<SelectWells>().Subscribe(ActiveWellChanged);
            eventAggregator.GetEvent<MacroRunEvent>().Subscribe(OnUpdateComponentList);
            eventAggregator.GetEvent<SaveAnalysisResultEvent>().Subscribe(OnSaveGraphics,ThreadOption.UIThread,true);
            eventAggregator.GetEvent<ShowRegionEvent>().Subscribe(OnSwitchTab);
            _activeWellNos = new List<int>();
            IsLoadGateEnd = true;
        }

        #endregion

        #region Methods

        #region Public interface

        private void Clear()
        {
            foreach (var graphicContainerVm in _graphicContainerVms)
            {
                graphicContainerVm.Clear();
            }
            _graphicContainerVms.Clear();
            _tabIdManager.Clear();
            _activeWellNos.Clear();
            ROIManager.Instance.Clear();
        }

        private void OnAddTab()
        {
            var name = ConstantHelper.DefaultTabName + _tabIdManager.GetId();
            var containerVm = new GraphicContainerVm(name);
            _graphicContainerVms.Add(containerVm);
            SelectedContainer = containerVm;
        }

        public bool IsValidateName(GraphicContainerVm vm, string name)
        {
            var result = true;
            foreach (var containerVm in _graphicContainerVms)
            {
                if (vm != containerVm && containerVm.Name == name)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        private void CheckTabNames()
        {
            foreach (var containerVm in _graphicContainerVms)
            {
                if (containerVm.Name.StartsWith(ConstantHelper.DefaultTabName) && containerVm.Name.Length > 3)
                {
                    var str = containerVm.Name.Remove(0, 3);
                    int id;
                    if (int.TryParse(str, out id))
                    {
                        _tabIdManager.InsertId(id);
                    }
                }
            }
        }

        private void OnDeleteTab()
        {
            if (_selectedContainer == null)
            {
                return;
            }
            if (_selectedContainer.Name.StartsWith(ConstantHelper.DefaultTabName))
            {
                var id = _selectedContainer.Name.Remove(0, 3);
                int idInt;
                if (int.TryParse(id, out idInt))
                {
                    _tabIdManager.RemoveId(idInt);
                }
            }

            var ids = ROIManager.Instance.GetRegionIdList();
            var regionList = new List<MaskRegion>();
            foreach (var graphicVm in _selectedContainer.GraphicVmList)
            {
                foreach (var regionId in ids)
                {
                    var region = ROIManager.Instance.GetRegion(regionId);
                    if (region != null && region.GraphicId == graphicVm.Id)
                    {
                        regionList.Add(region);
                    }
                }
                UpdateRegion(new RegionUpdateArgs(graphicVm.Id, regionList, RegionUpdateType.Delete));
                regionList.Clear();
            }
            _graphicContainerVms.Remove(_selectedContainer);
            if (_graphicContainerVms.Count > 0)
            {
                SelectedContainer = _graphicContainerVms[_graphicContainerVms.Count - 1];
            }
        }

        public List<GraphicVmBase> GetGraphicVmList()
        {
            var list = new List<GraphicVmBase>();
            foreach (var containerVm in _graphicContainerVms)
            {
                foreach (var graphicVm in containerVm.GraphicVmList)
                {
                    list.Add(graphicVm);
                }
            }
            return list;
        }

        public void Update(string graphicId)
        {
            var vmList = GetGraphicVmList();
            var vm = vmList.Find(graphicvm => graphicvm.Id == graphicId);
            if (vm == null)
            {
                return;
            }
            vm.UpdateEvents();
        }

        public string GetParentComponent(string graphicId)
        {
            var vmList = GetGraphicVmList();
            var vm = vmList.Find(graphicvm => graphicvm.Id == graphicId);
            if (vm == null)
            {
                return null;
            }
            return vm.SelectedComponent;
        }

        #endregion

        private void OnUpdateComponentList(int id)
        {
            var components = ComponentDataManager.Instance.GetComponentNames();
            if (components == null || components.Count == 0)
            {
                return;
            }
            foreach (var containerVm in _graphicContainerVms)
            {
                containerVm.IsNewGraphicEnabled = true;
                foreach (var graphicVm in containerVm.GraphicVmList)
                {
                    graphicVm.UpdateComponentList(components);
                    if (graphicVm.SelectedComponent != null)
                    {
                        graphicVm.UpdateFeatures();
                    }
                }
            }
        }

        private void ActiveWellChanged(List<int> wells)
        {
            AxisModel.IsSwitchWell = true;
            _activeWellNos = wells;
            _roiInstance.ChangeActiveWells(wells);
            var vmList = GetGraphicVmList();
            foreach (var graphicvm in vmList)
            {
                var histogram = graphicvm as HistogramVm;
                if (histogram == null)
                {
                    continue;
                }
                histogram.SetIsNewEnabld();
            }
            UpdateImage();
            AxisModel.IsSwitchWell = false;
        }

        private void OnSwitchTab(string tabName)
        {
            if (tabName == "AnalysisModule")
            {
                if (_activeWellNos.Count > 0)
                {
                    UpdateImage();
                }

            }
        }

        public Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>> GetGraphicDictionary()
        {
            var dic = new Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>>();
            foreach (var containerVm in _graphicContainerVms)
            {
                foreach (var tuple in containerVm.GraphicDictionary)
                {
                    dic.Add(tuple.Key, tuple.Value);
                }
            }
            return dic;
        }

        public void UpdateRegionLocation(string graphId)
        {
            var regionList = RegionHelper.GetRegionList();
            var list = new List<MaskRegion>();
            var dictionary = GetGraphicDictionary();
            foreach (var regionItem in regionList)
            {
                if (regionItem.GraphicId != graphId)
                {
                    continue;
                }
                var id = ConstantHelper.PrefixRegionName + regionItem.Id;
                UpdateRegionPoint(id, regionItem, dictionary[regionItem.GraphicId].Item2,
                    dictionary[regionItem.GraphicId].Item1);
                list.Add(regionItem);
            }
            if (list.Count > 0)
            {
                _roiInstance.UpdateRegions(list);
            }
        }

        public void UpdateGraphFeatures(string graphId)
        {
            var regionList = RegionHelper.GetRegionList();
            var list = new List<MaskRegion>();

            var dictionary = GetGraphicDictionary();
            if (dictionary.ContainsKey(graphId))
            {
                var scatterVm = dictionary[graphId].Item2 as ScattergramVm;
                if (scatterVm != null)
                {
                    scatterVm.CheckNormalizeXyEnabled();
                }
            }
            foreach (var regionItem in regionList)
            {
                var id = ConstantHelper.PrefixRegionName + regionItem.Id;
                var scattergramVm = dictionary[regionItem.GraphicId].Item2 as ScattergramVm;
                UpdateRegionPoint(id, regionItem, dictionary[regionItem.GraphicId].Item2,
                    dictionary[regionItem.GraphicId].Item1);

                if (scattergramVm != null)
                {
                    RegionHelper.Set2DCommonRegionParas(regionItem, scattergramVm);
                }
                else
                {
                    RegionHelper.SetCommonRegionParas(regionItem, dictionary[regionItem.GraphicId].Item2);
                }
                list.Add(regionItem);
            }
            var count = list.Count(region => region.GraphicId.Equals(graphId));
            if (count == 0)
            {
                var vm = dictionary[graphId].Item2;
                if (vm != null)
                {
                    vm.UpdateEvents();
                    return;
                }
            }
            if (list.Count > 0)
            {
                _roiInstance.UpdateRegions(list);
            }
            UpdateImage();
        }

        private void UpdateImage()
        {
            if (_activeWellNos == null)
            {
                return;
            }
            foreach (var container in _graphicContainerVms)
            {
                container.UpdateImage();
            }
        }

        private void UpdateRegion(RegionUpdateArgs args)
        {
            var regions = args.RegionList.ToList();
            var regionIds = regions.Select(region => string.Format("R{0}", region.Id)).ToList();
            switch (args.UpdateType)
            {
                case RegionUpdateType.Update:
                case RegionUpdateType.Color:
                    if (regions.Count > 0)
                    {
                        _roiInstance.UpdateRegions(regions);
                    }
                    break;
                case RegionUpdateType.Delete:
                    _roiInstance.RemoveRegions(regionIds);
                    UpdateRegionList();
                    break;
                case RegionUpdateType.Add:
                    foreach (var region in regions)
                    {
                        _roiInstance.AddRegion(region);
                    }
                    UpdateRegionList();
                    break;
            }

            if (args.UpdateType != RegionUpdateType.Add)
            {
                UpdateImage();
            }
        }

        public void UpdateRelationShip(string graphId)
        {
            RegionHelper.UpdateRelationShip(graphId);
            if (IsLoadGateEnd)
            {
                UpdateImage();
            }
        }

        public void UpdateRegionList()
        {
            var vmList = new List<GraphicVmBase>();
            foreach (var containervm in _graphicContainerVms)
            {
                foreach (var tuple in containervm.GraphicDictionary)
                {
                    vmList.Add(tuple.Value.Item2);
                }
            }
            if (vmList.Count == 0)
            {
                return;
            }
            UpdateGateList(vmList);
        }

        private void UpdateGateList(IEnumerable<GraphicVmBase> vmList)
        {
            var regionList = RegionHelper.GetRegionList();
            foreach (var vm in vmList)
            {
                var selfRegionList = RegionHelper.GetSelfRegionList(regionList, vm.Id);
                var descendantList = RegionHelper.GetDescendants(regionList, selfRegionList);
                var gate1 = vm.SelectedGate1;
                var gate2 = vm.SelectedGate2;
                var list = new ImpObservableCollection<string>();
                if (selfRegionList.Count == 0)
                {
                    var gateList = new ImpObservableCollection<string>();
                    gateList.AddRange(regionList.Select(region => ConstantHelper.PrefixRegionName + region.Id).ToList());
                    vm.Gate1List = gateList;
                    vm.Gate2List = (ImpObservableCollection<string>)gateList.Clone();
                }
                else
                {
                    foreach (var region in regionList)
                    {
                        if (!descendantList.Contains(ConstantHelper.PrefixRegionName + region.Id))
                        {
                            list.Add(ConstantHelper.PrefixRegionName + region.Id);
                        }
                    }
                    vm.Gate1List = list;
                    vm.Gate2List = (ImpObservableCollection<string>)list.Clone();
                }

                var noneString = OperationType.None.ToString();
                if (!vm.Gate1List.Contains(noneString))
                {
                    vm.Gate1List.Insert(0, OperationType.None.ToString());
                }
                if (vm.Gate2List.Contains(noneString))
                {
                    vm.Gate2List.Remove(noneString);
                }
                vm.SelectedGate1 = vm.Gate1List.Contains(gate1) ? gate1 : vm.Gate1List[0];
                vm.SelectedGate2 = gate2 != null && vm.Gate2List.Contains(gate2) ? gate2 : null;
            }
        }

        private void UpdateRegionPoint(string id, MaskRegion region, GraphicVmBase vm, GraphicUcBase graph)
        {
            var graphic = graph.RegionPanel.GetGraphic(id);
            if (graphic == null)
            {
                return;
            }
            RegionHelper.UpdateRegionLocation(region, graphic, graph.RegionPanel, vm);
        }

        public void UpdateRegionPoint(GraphicVmBase vm)
        {
            var regions = RegionHelper.GetRegionList();
            var dic = GetGraphicDictionary();
            if (!dic.ContainsKey(vm.Id))
            {
                return;
            }
            var list = new List<MaskRegion>();
            foreach (var region in regions)
            {
                if (vm.Id == region.GraphicId)
                {
                    var graphic = dic[vm.Id].Item1.RegionPanel.GetGraphic(string.Format("R{0}", region.Id));
                    if (graphic == null)
                    {
                        continue;
                    }
                    RegionHelper.UpdateRegionLocation(region, graphic, dic[vm.Id].Item1.RegionPanel, vm);
                    list.Add(region);
                }
            }
            if (list.Count > 0)
            {
                _roiInstance.UpdateRegions(list);
            }
        }

        private void SetRegion(GraphicVmBase vm, ref MaskRegion region, bool isScattergram = true)
        {
            region.GraphicId = vm.Id;
            region.ComponentName = vm.SelectedComponent;
            if (isScattergram)
            {
                RegionHelper.Set2DCommonRegionParas(region, (ScattergramVm)vm);
            }
            else
            {
                RegionHelper.SetCommonRegionParas(region, vm);
            }
        }

        #region Load Xml

        public void LoadXml(int scanid)
        {
            var experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            if (experiment == null)
            {
                return;
            }
            Clear();
            var info = experiment.GetExperimentInfo();
            if (info.LoadWithAnalysisResult == false)
            {
                if (_graphicContainerVms.Count == 0)
                {
                    OnAddTab();
                }
                return;
            }
            var filepath = info.AnalysisPath;
            var path = Path.Combine(filepath, ConstantHelper.GraphicXmlPath);
          
            var reader = new XmlTextReader(path);
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "graphs":
                            LoadGraphs(reader);
                            break;
                        case "gates":
                            LoadGates(reader);
                            break;
                        case "subspaces":
                            LoadSubspace(reader);
                            break;
                    }
                }

            }
            if (_graphicContainerVms.Count == 0)
            {
                OnAddTab();
            }
        }

        private void LoadSubspace(XmlReader reader)
        {
            GraphicContainerVm containerVm = null;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "subspace")
                    {
                        containerVm = new GraphicContainerVm(reader["name"]);
                        _graphicContainerVms.Add(containerVm);
                    }
                    else if (reader.Name == "info")
                    {
                        var s = reader["name"];
                        if (s != null)
                        {
                            var id = s.Remove(0, 1);
                            var graphicVm = _tempGraphicvmList.Find(vm => vm.Id == id);
                            if (containerVm != null && graphicVm != null)
                            {
                                containerVm.GraphicVmList.Add(graphicVm);
                                var isScatter = (graphicVm as ScattergramVm) != null;
                                GraphicUcBase graphicUserControl;
                                if (isScatter)
                                {
                                    graphicUserControl = new ScattergramView { DataContext = graphicVm };
                                    graphicVm.ViewDispatcher = graphicUserControl.Dispatcher;
                                    containerVm.GraphicDictionary.Add(id, new Tuple<GraphicUcBase, GraphicVmBase>(graphicUserControl, graphicVm));
                                }
                                else
                                {
                                    graphicUserControl = new HistogramView { DataContext = graphicVm };
                                    graphicVm.ViewDispatcher = graphicUserControl.Dispatcher;
                                    containerVm.GraphicDictionary.Add(id, new Tuple<GraphicUcBase, GraphicVmBase>(graphicUserControl, graphicVm));
                                }
                                if (containerVm.GraphicVmList.Count == 1)
                                {
                                    containerVm.SelectedGraphic = graphicVm;
                                }
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "subspace")
                    {
                        RegionHelper.InitRegionRelationship();
                        UpdateGateList(_tempGraphicvmList);
                    }
                }
            }
            if (_graphicContainerVms.Count > 0)
            {
                SelectedContainer = _graphicContainerVms[0];
            }
            _tempGraphicvmList.Clear();
            CheckTabNames();
        }


        public void LoadGraphs(XmlReader reader)
        {
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "scattergram":
                            var vm = new ScattergramVm();
                            LoadScattergram(reader, vm);
                            _tempGraphicvmList.Add(vm);
                            vm.SetTitle();
                            vm.CheckNormalizeXyEnabled();
                            break;
                        case "histogram":
                            var histogramVm = new HistogramVm();
                            LoadHistogram(reader, histogramVm);
                            _tempGraphicvmList.Add(histogramVm);
                            histogramVm.SetTitle();
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "graphs")
                    {
                        break;
                    }
                }
            }
        }

        public void LoadGates(XmlReader reader)
        {
            if (reader.IsEmptyElement)
            {
                return;
            }
            IsLoadGateEnd = false;
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "graph")
                    {
                        if (reader["ref-id"] == null)
                        {
                            break;
                        }
                        var id = reader["ref-id"];
                        foreach (var graphicvm in _tempGraphicvmList)
                        {

                            if (graphicvm.Id == id)
                            {
                                if (reader["gate1"] != null)
                                {
                                    graphicvm.SelectedGate1 = ConstantHelper.PrefixRegionName + reader["gate1"];
                                }
                                if (reader["op"] != null)
                                {
                                    var op = reader["op"];
                                    OperationType type;
                                    if (Enum.TryParse(op, true, out type))
                                    {
                                        graphicvm.SelectedOperator = type;
                                    }

                                }
                                if (reader["gate2"] != null)
                                {
                                    graphicvm.SelectedGate2 = ConstantHelper.PrefixRegionName + reader["gate2"];
                                }
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "gates")
                    {
                        IsLoadGateEnd = true;
                        break;
                    }
                }
            }
        }

        private void LoadHistogram(XmlReader reader, HistogramVm vm)
        {
            vm.IsInitialized = false;
            vm.XAxis.IsInitialized = false;
            vm.YAxis.IsInitialized = false;
            vm.Init();
            if (reader["component"] != null)
            {
                vm.SelectedComponent = reader["component"];
            }

            if (reader["graph-id"] != null)
            {
                var id = reader["graph-id"];
                int result;
                if (int.TryParse(id, out result))
                {
                    GraphicContainerVm.IdManagerInstance.InsertId(result);
                }
                vm.Id = id;
                vm.SetTitle();
                vm.XAxis.GraphicId = id;
                vm.YAxis.GraphicId = id;
            }
            if (reader["style"] != null)
            {
                GraphStyle style;
                if (Enum.TryParse(reader["style"], true, out style))
                {
                    vm.GraphType = style;
                }
            }
            if (reader["auto-yscale"] != null)
            {
                vm.IsAutoYScale = XmlConvert.ToBoolean(reader["auto-yscale"]);
            }
            if (reader["yscale"] != null)
            {
                vm.YScaleValue = XmlConvert.ToInt32(reader["yscale"]);
            }
            if (reader["smooth"] != null)
            {
                vm.Smooth = XmlConvert.ToDouble(reader["smooth"]);
            }
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "xaxis":
                            ReadAxis(reader, vm.XAxis);
                            break;
                        case "region":
                            LoadRegion(reader, vm, false);
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "histogram")
                    {
                        vm.IsInitialized = true;
                        vm.XAxis.IsInitialized = true;
                        vm.YAxis.IsInitialized = true;
                        break;
                    }
                }
            }
        }

        private void LoadScattergram(XmlReader reader, ScattergramVm vm)
        {
            vm.IsInitialized = false;
            vm.XAxis.IsInitialized = false;
            vm.YAxis.IsInitialized = false;
            vm.Init();
            if (reader["component"] != null)
            {
                vm.SelectedComponent = reader["component"];
            }

            if (reader["graph-id"] != null)
            {
                var id = reader["graph-id"];
                int result;
                if (int.TryParse(id, out result))
                {
                    GraphicContainerVm.IdManagerInstance.InsertId(result);
                }
                vm.Id = id;
                vm.SetTitle();
                vm.XAxis.GraphicId = id;
                vm.YAxis.GraphicId = id;
            }

            if (reader["style"] != null)
            {
                // bool value = reader["style"] == "DensityMap";
                GraphStyle style;
                if (Enum.TryParse(reader["style"], true, out style))
                {
                    vm.GraphType = style;
                    if (style == GraphStyle.DensityMap)
                    {
                        vm.IsMapChecked = vm.IsDensity = true;
                        vm.IsExpression = false;
                    }
                    else if (style == GraphStyle.ValueMap)
                    {
                        vm.IsMapChecked = vm.IsExpression = true;
                        vm.IsDensity = false;
                    }
                }
            }

            if (reader["normalize-xy"] != null)
            {
                vm.IsNormalizexy = XmlConvert.ToBoolean(reader["normalize-xy"]);
            }
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.Name)
                    {
                        case "quadrant":
                            var x = double.NaN;
                            var y = double.NaN;
                            if (reader["x"] != null)
                            {
                                x = XmlConvert.ToDouble(reader["x"]);
                            }
                            if (reader["y"] != null)
                            {
                                y = XmlConvert.ToDouble(reader["y"]);
                            }
                            if (!double.IsNaN(x) && !double.IsNaN(y))
                            {
                                vm.QuadrantCenterPoint = new Point(x, y);
                            }
                            vm.IsShowQuadrant = true;
                            break;
                        case "density-map":
                            if (reader["min"] != null)
                            {
                                vm.ZScaleMin = (float)XmlConvert.ToDouble(reader["min"]);
                            }
                            if (reader["max"] != null)
                            {
                                vm.ZScaleMax = (float)XmlConvert.ToDouble(reader["max"]);
                            }
                            break;
                        case "xaxis":
                            ReadAxis(reader, vm.XAxis);
                            break;
                        case "yaxis":
                            ReadAxis(reader, vm.YAxis);
                            break;
                        case "region":
                            LoadRegion(reader, vm);
                            break;
                        case "value-map":
                            vm.ZScaleMin = Convert.ToSingle(reader["min"]);
                            vm.ZScaleMax = Convert.ToSingle(reader["max"]);
                            foreach (var feature in vm.ZScaleFeatureList)
                            {
                                if (feature.Name == reader["feature"])
                                {
                                    vm.SelectedZScaleFeature = feature;
                                }
                            }

                            foreach (var channel in vm.ZScaleChannelList)
                            {
                                if (channel.ChannelId.ToString(CultureInfo.InvariantCulture) == reader["channel"])
                                {
                                    vm.SelecedZScaleChannel = channel;
                                }
                            }
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == "scattergram")
                    {
                        vm.IsInitialized = true;
                        vm.XAxis.IsInitialized = true;
                        vm.YAxis.IsInitialized = true;
                        break;
                    }
                }
            }
        }

        private void ReadAxis(XmlReader reader, AxisModel axis)
        {
            if (reader["def-label"] != null)
            {
                axis.IsDefaultLabel = XmlConvert.ToBoolean(reader["def-label"]);
            }

            if (reader["label"] != null)
            {
                axis.LabelString = reader["label"];
            }

            if (reader["min"] != null)
            {
                axis.MinRange = XmlConvert.ToSingle(reader["min"]);
            }

            if (reader["max"] != null)
            {
                axis.MaxRange = XmlConvert.ToSingle(reader["max"]);
            }
            if (reader["log"] != null)
            {
                axis.IsLogScale = XmlConvert.ToBoolean(reader["log"]);
            }
            if (reader["normalize"] != null)
            {
                axis.IsNormalize = XmlConvert.ToBoolean(reader["normalize"]);
            }
            // read parameter
            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.Name == "numerator")
                    {
                        foreach (var feature in axis.NumeratorFeatureList)
                        {
                            if (feature.Name == reader["feature"])
                            {
                                axis.SelectedNumeratorFeature = feature;
                            }
                        }

                        foreach (var channel in axis.NumeratorChannelList)
                        {
                            if (channel.ChannelId.ToString(CultureInfo.InvariantCulture) == reader["channel"])
                            {
                                axis.SelectedNumeratorChannel = channel;
                            }
                        }
                    }
                    else if (reader.Name == "denominator")
                    {
                        foreach (var feature in axis.DenominatorFeatureList)
                        {
                            if (feature.Name == reader["feature"])
                            {
                                axis.SelectedDenominatorFeature = feature;
                            }
                        }

                        foreach (var channel in axis.DenominatorChannelList)
                        {
                            if (channel.ChannelId.ToString(CultureInfo.InvariantCulture) == reader["channel"])
                            {
                                axis.SelectedNumeratorChannel = channel;
                            }
                        }
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && (reader.Name == "xaxis" || reader.Name == "yaxis"))
                    // end of axis
                    break;
            }
        }

        private void LoadRegion(XmlReader reader, GraphicVmBase vm, bool isScattergram = true)
        {
            var shape = reader["shape"];
            var name = ConstantHelper.PrefixRegionName;
            var color = Colors.White;
            MaskRegion region = null;
            var xscale = ConstantHelper.LowBinCount / (vm.XAxis.MaxValue - vm.XAxis.MinValue);
            var yscale = ConstantHelper.LowBinCount / (vm.YAxis.MaxValue - vm.YAxis.MinValue);

            if (string.IsNullOrEmpty(shape))
            {
                return;
            }

            if (reader["id"] != null)
            {
                name += reader["id"];
            }
            if (reader["color"] != null)
            {
                var convertFromString = ColorConverter.ConvertFromString(reader["color"]);
                if (convertFromString != null)
                    color = (Color)convertFromString;
            }
            var id = int.Parse(name.Remove(0, 1));
            switch (shape)
            {
                case "Range":
                case "Rectangle":
                case "Ellipse":
                    var x = double.NaN;
                    var y = double.NaN;
                    var width = double.NaN;
                    var height = double.NaN;

                    if (reader["x"] != null)
                    {
                        x = XmlConvert.ToDouble(reader["x"]);
                    }

                    if (reader["y"] != null)
                    {
                        y = ConstantHelper.LowBinCount - XmlConvert.ToDouble(reader["y"]);
                    }
                    if (!double.IsNaN(x) && !double.IsNaN(y))
                    {
                        var pt = new Point(x, y);
                        var left = pt.X / xscale;
                        var top = pt.Y / yscale;
                        if (reader["w"] != null)
                        {
                            width = XmlConvert.ToDouble(reader["w"]) / xscale;
                        }
                        if (reader["h"] != null)
                        {
                            height = XmlConvert.ToDouble(reader["h"]) / yscale;
                        }
                        var rect = new Rect(left, top, width, height);


                        if (shape == "Rectangle")
                        {
                            region = new RectangleRegion(id, rect.Size, rect.TopLeft);
                        }
                        else if (shape == "Range")
                        {
                            region = new GateRegion(id, rect.Left, rect.Right);
                        }
                        else
                        {
                            var center = new Point(rect.Left + rect.Width / 2.0,
                                rect.Top - rect.Height / 2.0);
                            region = new EllipseRegion(id, rect.Size, center);
                        }

                    }
                    break;
                case "Polygon":
                    var list = ReadPolygon(reader, vm);
                    if (list != null)
                    {
                        region = new PolygonRegion(id, list);
                    }
                    break;
            }
            if (region != null)
            {
                SetRegion(vm, ref region, isScattergram);
                region.Color = color;
                _roiInstance.SetRegion(region);
            }
        }


        private IEnumerable<Point> ReadPolygon(XmlReader reader, GraphicVmBase vm)
        {
            var points = new List<Point>();
            var x = double.NaN;
            var y = double.NaN;
            var xscale = ConstantHelper.LowBinCount / (vm.XAxis.MaxValue - vm.XAxis.MinValue);
            var yscale = ConstantHelper.LowBinCount / (vm.YAxis.MaxValue - vm.YAxis.MinValue);

            while (reader.Read())
            {
                if (reader.Name == "point")
                {
                    if (reader["x"] != null)
                    {
                        x = XmlConvert.ToDouble(reader["x"]) / xscale + vm.XAxis.MinValue;
                    }
                    if (reader["y"] != null)
                    {
                        y = (ConstantHelper.LowBinCount - XmlConvert.ToDouble(reader["y"])) / yscale + vm.YAxis.MinValue;
                    }

                    if (!double.IsNaN(x) && !double.IsNaN(y))
                    {
                        var p = new Point(x, y);
                        points.Add(p);
                    }
                }

                if (reader.Name == "region" && reader.NodeType == XmlNodeType.EndElement)
                {
                    return points;
                }
            }
            return points;
        }


        #endregion

        #region Save Xml

        private void OnSaveGraphics(int i)
        {
            var experiment = ServiceLocator.Current.GetInstance<IExperiment>();
            if (experiment == null)
            {
                return;
            }
            var xmlPath = experiment.GetExperimentInfo().AnalysisPath;
            if (!Directory.Exists(xmlPath))
                Directory.CreateDirectory(xmlPath);
            var filepath = Path.Combine(xmlPath, ConstantHelper.GraphicXmlPath);
            if (File.Exists(filepath))
            {
                File.Delete(filepath);
            }
            var graphicDic = GetGraphicDictionary();
            var xe = new XElement("display-protocol");
            var graphs = new XElement("graphs");
            var gates = new XElement("gates");
            var subspaces = new XElement("subspaces");
            xe.Add(graphs);
            xe.Add(gates);
            xe.Add(subspaces);
            SaveGraphics(graphs, graphicDic);
            SaveGates(gates);
            SaveSubSpaces(subspaces);
            xe.Save(filepath);
        }

        private void SaveGraphics(XElement graphs, Dictionary<string, Tuple<GraphicUcBase, GraphicVmBase>> graphicDic)
        {
            foreach (var graphicTuple in graphicDic)
            {
                XElement graphic;
                var vm = graphicTuple.Value.Item2;
                var scattergramVm = vm as ScattergramVm;

                IList<XAttribute> attributeList = new List<XAttribute>
                    {
                        new XAttribute("graph-id", vm.Id),
                        new XAttribute("component", vm.SelectedComponent ?? string.Empty),
                        new XAttribute("normalize-xy", vm.IsNormalizexy)
                    };
                string style;
                if ((scattergramVm != null))
                {
                    if (scattergramVm.IsMapChecked)
                    {
                        if (scattergramVm.IsDensity)
                        {
                            style = "DensityMap";
                        }
                        else
                        {
                            style = "ValueMap";
                        }
                    }
                    else
                    {
                        style = "DotPlot";
                    }

                    attributeList.Add(new XAttribute("style", style));
                    graphic = new XElement("scattergram", attributeList);

                }
                else
                {
                    var histogramVm = (HistogramVm)vm;
                    attributeList.Add(new XAttribute("auto-yscale", histogramVm.IsAutoYScale));
                    attributeList.Add(new XAttribute("yscale", histogramVm.YScaleValue));
                    if (histogramVm.GraphType == GraphStyle.BarChart)
                    {
                        style = "BarChart";
                    }
                    else
                    {
                        style = "Outline";
                    }
                    attributeList.Add(new XAttribute("style", style));
                    graphic = new XElement("histogram", attributeList);
                }

                if (scattergramVm != null)
                {
                    if (scattergramVm.IsShowQuadrant)
                    {
                        var scattergram = graphicTuple.Value.Item1.RegionPanel as Scattergram;
                        if (scattergram != null)
                        {
                            var point = scattergram.GetQuadrantPoint();
                            point = new Point((int)(point.X / scattergram.XScale), (int)(point.Y / scattergram.YScale));
                            var quadrant = new XElement("quadrant", new XAttribute("x", point.X), new XAttribute("y", point.Y));
                            graphic.Add(quadrant);
                        }
                    }
                    if (scattergramVm.IsDensity)
                    {
                        attributeList = new List<XAttribute>
                        {
                            new XAttribute("min",scattergramVm.ZScaleMin ),
                            new XAttribute("max", scattergramVm.ZScaleMax),
                         };
                        var density = new XElement("density-map", attributeList);
                        graphic.Add(density);
                    }
                    if (scattergramVm.IsExpression)
                    {
                        attributeList = new List<XAttribute>
                        {
                            new XAttribute("feature",scattergramVm.SelectedZScaleFeature!=null?scattergramVm.SelectedZScaleFeature.Name:string.Empty ),
                            new XAttribute("channel", scattergramVm.SelecedZScaleChannel != null ? scattergramVm.SelecedZScaleChannel.ChannelId.ToString():string.Empty),
                            new XAttribute("min",scattergramVm.ZScaleMin ),
                            new XAttribute("max", scattergramVm.ZScaleMax)
                         };
                        var expression = new XElement("value-map", attributeList);
                        graphic.Add(expression);
                    }
                }

                attributeList = new List<XAttribute>
                    {
                        new XAttribute("def-label", vm.XAxis.IsDefaultLabel),
                        new XAttribute("label", vm.XAxis.LabelString),
                        new XAttribute("min", vm.XAxis.MinRange),
                        new XAttribute("max", vm.XAxis.MaxRange),
                        new XAttribute("log", vm.XAxis.IsLogScale),
                        new XAttribute("normalize", vm.XAxis.IsNormalize)
                    };
                var xAxis = new XElement("xaxis", attributeList);
                attributeList = new List<XAttribute>
                    {
                        new XAttribute("feature", vm.XAxis.SelectedNumeratorFeature != null ? vm.XAxis.SelectedNumeratorFeature.Name:string.Empty),
                        new XAttribute("channel", vm.XAxis.SelectedNumeratorChannel!= null ? vm.XAxis.SelectedNumeratorChannel.ChannelId.ToString(): string.Empty),
                    };
                var numerator = new XElement("numerator", attributeList);
                xAxis.Add(numerator);
                if (vm.XAxis.SelectedDenominatorFeature != null)
                {
                    attributeList = new List<XAttribute>
                    {
                        new XAttribute("feature", vm.XAxis.SelectedDenominatorFeature.Name),
                        new XAttribute("channel", vm.XAxis.SelectedDenominatorChannel!= null ? vm.XAxis.SelectedDenominatorChannel.ChannelId.ToString(): string.Empty),
                    };
                    var denominator = new XElement("denominator", attributeList);
                    xAxis.Add(denominator);
                }

                graphic.Add(xAxis);
                if (scattergramVm != null)
                {
                    attributeList = new List<XAttribute>
                    {
                        new XAttribute("def-label", vm.YAxis.IsDefaultLabel),
                        new XAttribute("label", vm.YAxis.LabelString),
                        new XAttribute("min", vm.YAxis.MinRange),
                        new XAttribute("max", vm.YAxis.MaxRange),
                        new XAttribute("log", vm.YAxis.IsLogScale),
                        new XAttribute("normalize", vm.YAxis.IsNormalize)
                    };
                    var yAxis = new XElement("yaxis", attributeList);
                    attributeList = new List<XAttribute>
                    {
                        new XAttribute("feature", vm.YAxis.SelectedNumeratorFeature != null ? vm.YAxis.SelectedNumeratorFeature.Name:string.Empty),
                        new XAttribute("channel", vm.YAxis.SelectedNumeratorChannel!= null ? vm.YAxis.SelectedNumeratorChannel.ChannelId.ToString(): string.Empty),
                    };
                    numerator = new XElement("numerator", attributeList);
                    yAxis.Add(numerator);
                    if (vm.YAxis.SelectedDenominatorFeature != null)
                    {
                        attributeList = new List<XAttribute>
                    {
                        new XAttribute("feature", vm.YAxis.SelectedDenominatorFeature.Name),
                        new XAttribute("channel", vm.YAxis.SelectedDenominatorChannel!= null ? vm.YAxis.SelectedDenominatorChannel.ChannelId.ToString(): string.Empty),
                    };
                        var denominator = new XElement("denominator", attributeList);
                        yAxis.Add(denominator);
                    }

                    graphic.Add(yAxis);
                }
                SaveRegions(graphic, graphicTuple.Value.Item1.RegionPanel);
                graphs.Add(graphic);
            }

        }

        private void SaveRegions(XElement graphic, RegionCanvas regionCanvas)
        {
            if (graphic == null || regionCanvas == null)
            {
                return;
            }
            var xscale = regionCanvas.ActualWidth / ConstantHelper.LowBinCount;
            var yscale = regionCanvas.ActualHeight / ConstantHelper.LowBinCount;

            foreach (var visual in regionCanvas.VisualList)
            {
                var g = visual as GraphicsBase;
                if (g == null)
                    continue;

                XElement region;
                IList<XAttribute> attributeList;
                switch (g.GraphicType)
                {
                    case RegionType.Rectangle:
                    case RegionType.Ellipse:
                        var rect = (GraphicsRectangleBase)g;
                        var color = g.ObjectColor != Colors.Black
                            ? GraphicVmBase.ColorRegionList.First(colorModel => colorModel.RegionColor == g.ObjectColor)
                            .RegionColorString : "Black";
                        string shape = string.Empty;
                        if (rect.GraphicType == RegionType.Rectangle)
                        {
                            shape = (regionCanvas as Scattergram) != null ? "Rectangle" : "Range";
                        }
                        else if (rect.GraphicType == RegionType.Ellipse)
                        {
                            shape = "Ellipse";
                        }

                        attributeList = new List<XAttribute>
                        {
                            new XAttribute("id", rect.Name.Remove(0, 1)),
                            new XAttribute("shape", shape),
                            new XAttribute("color", color == ConstantHelper.DefaultColorStr ? "white":color),
                            new XAttribute("x", (int)(rect.Left/xscale)),
                            new XAttribute("y", (int)(rect.Top/yscale)),
                            new XAttribute("w", (int)(rect.Rectangle.Width/xscale)),
                            new XAttribute("h", shape == "Range"?  200:(int)(rect.Rectangle.Height / yscale))
                        };
                        region = new XElement("region", attributeList);
                        graphic.Add(region);
                        break;
                    case RegionType.Polygon:
                        var polygon = (GraphicsPolygon)g;
                        var polygonColor = g.ObjectColor != Colors.Black
                            ? GraphicVmBase.ColorRegionList.First(colorModel => colorModel.RegionColor == g.ObjectColor)
                        .RegionColorString : "Black";
                        attributeList = new List<XAttribute>
                        {
                            new XAttribute("id", polygon.Name.Remove(0, 1)),
                            new XAttribute("shape", "Polygon"),
                            new XAttribute("color", polygonColor == ConstantHelper.DefaultColorStr ? "white":polygonColor),
                        };
                        region = new XElement("region", attributeList);

                        var points = new XElement("points", new XAttribute("count", polygon.Points.Count()));
                        foreach (var pt in polygon.Points)
                        {
                            var point = new XElement("point", new XAttribute("x", (int)(pt.X / xscale)), new XAttribute("y", (int)(pt.Y / yscale)));
                            points.Add(point);
                        }
                        region.Add(points);
                        graphic.Add(region);
                        break;
                }
            }
        }

        private void SaveGates(XElement gates)
        {
            var vmList = GetGraphicVmList();
            if (gates == null || vmList.Count == 0)
            {
                return;
            }

            var attributeList = new List<XAttribute>();

            foreach (var graphicVm in vmList)
            {
                if (!string.IsNullOrEmpty(graphicVm.SelectedGate1) && graphicVm.SelectedGate1.StartsWith(ConstantHelper.PrefixRegionName))
                {
                    attributeList.Add(new XAttribute("gate1", graphicVm.SelectedGate1.Remove(0, 1)));
                }
                if (!string.IsNullOrEmpty(graphicVm.SelectedGate2) && graphicVm.SelectedGate2.StartsWith(ConstantHelper.PrefixRegionName))
                {
                    attributeList.Add(new XAttribute("gate2", graphicVm.SelectedGate2.Remove(0, 1)));
                }
                if (attributeList.Count > 0)
                {
                    attributeList.Insert(0, new XAttribute("ref-id", graphicVm.Id));
                    gates.Add(new XElement("graph", attributeList));
                }
                attributeList.Clear();
            }
        }

        private void SaveSubSpaces(XElement subspaces)
        {
            if (subspaces == null || _graphicContainerVms.Count == 0)
            {
                return;
            }

            foreach (var containerVm in _graphicContainerVms)
            {
                var subspace = new XElement("subspace", new XAttribute("name", containerVm.Name));
                foreach (var graphiVm in containerVm.GraphicVmList)
                {
                    var name = ((graphiVm as ScattergramVm) != null
                        ? ConstantHelper.PrefixScattergramName
                        : ConstantHelper.PrefixHistogramName) + graphiVm.Id;
                    var info = new XElement("info", new XAttribute("name", name));
                    subspace.Add(info);
                }
                subspaces.Add(subspace);
            }
        }
        #endregion

        #endregion
    }
}
