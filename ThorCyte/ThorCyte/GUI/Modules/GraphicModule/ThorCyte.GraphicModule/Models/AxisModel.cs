using System;
using System.Collections;
using System.Text;
using System.Windows;
using ComponentDataService.Types;
using Prism.Mvvm;
using ThorCyte.GraphicModule.Helper;
using ThorCyte.GraphicModule.Infrastructure;
using ThorCyte.GraphicModule.Utils;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.GraphicModule.Models
{
    public class AxisModel : BindableBase, ICloneable
    {
        #region Properties and Fields

        public const string DefaultFeature = "(None)";

        private long[] _logTable = new long[ConstantHelper.LowBinCount + 1];

        public static bool IsSwitchWell;

        public string GraphicId { get; set; }

        public int NumeratorFeatureIndex { get; set; }

        public int DenominatorFeatureIndex { get; set; }

        private int _binCount;

        public int BinCount
        {
            get { return _binCount; }
            set
            {
                _binCount = value;
                if (_isLogScale)
                {
                    InitLogTable();
                }
            }
        }

        public long[] LogTable
        {
            get { return _logTable; }
        }

        private bool _isDefaultLabel;

        public bool IsDefaultLabel
        {
            get { return _isDefaultLabel; }
            set
            {
                if (_isDefaultLabel == value)
                {
                    return;
                }
                _isDefaultLabel = value;
                SetProperty(ref _isDefaultLabel, value);
                UpdateTitle();
            }
        }

        private bool _isLogScale;

        public bool IsLogScale
        {
            get { return _isLogScale; }
            set
            {
                if (_isLogScale == value)
                {
                    return;
                }

                SetProperty(ref _isLogScale, value);
                if (value)
                {
                    OldMinRange = _minRange;
                    OldMaxRange = _maxRange;
                    SetDefaultRange(true);
                }
                else
                {
                    SetOldRange();
                }
                OnFeatureUpdate();
            }
        }

        private bool _isNormalize;

        public bool IsNormalize
        {
            get { return _isNormalize; }
            set
            {
                if (_isNormalize == value)
                {
                    return;
                }
                _isNormalize = value;
                IsRangeEnabled = !value;
                if (!value)
                {
                    MinRange = OldMinRange;
                    MaxRange = OldMaxRange;
                }
                GraphicModule.GraphicManagerVmInstance.NormalizeToActiveWell(GraphicId, this, value);
                SetProperty(ref _isNormalize, value);
            }
        }

        public double OldMinRange { get; set; }
        protected double _minRange;

        public double MinRange
        {
            get { return _minRange; }
            set
            {
                MinValue = _isLogScale ? Math.Pow(10, value) : value;
                if (Math.Abs(_minRange - value) < double.Epsilon)
                {
                    return;
                }
                SetProperty(ref _minRange, value);
                OnFeatureUpdate();

            }
        }

        public double OldMaxRange { get; set; }
        private double _maxRange;

        public double MaxRange
        {
            get { return _maxRange; }
            set
            {
                if (Math.Abs(_maxRange - value) < double.Epsilon)
                {
                    return;
                }
                MaxValue = _isLogScale ? Math.Pow(10, value) : value;
                SetProperty(ref _maxRange, value);
                OnFeatureUpdate();
            }
        }

        private double _minValue;

        public double MinValue
        {
            get { return _minValue; }
            set { _minValue = value; }
        }

        private double _maxValue;

        public double MaxValue
        {
            get { return _maxValue; }
            set {_maxValue = value; }
        }

        private string _label;

        public string Label
        {
            get { return _label; }
            set
            {
                if (_label == value)
                {
                    return;
                }
                SetProperty(ref _label, value);
            }
        }


        private bool _isSelected;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected == value)
                {
                    return;
                }
                SetProperty(ref _isSelected, value);
            }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                {
                    return;
                }
                SetProperty(ref _title, value);
            }
        }

        public AxesEnum AxisType { get; set; }

        public bool IsInitialized { get; set; }

        public bool IsMaxCountChanged { get; set; }

        private Feature _selectedNumeratorFeature;

        public Feature SelectedNumeratorFeature
        {
            get { return _selectedNumeratorFeature; }
            set
            {
                if (_selectedNumeratorFeature == value)
                {
                    return;
                }
                _selectedNumeratorFeature = value;
                if (_selectedNumeratorFeature != null)
                {
                    if (_selectedNumeratorFeature.IsPerChannel)
                    {
                        IsNumeratorChannelEnabled = true;
                        _selectedNumeratorChannel = _numeratorChannelList[0];
                    }
                    else
                    {
                        IsNumeratorChannelEnabled = false;
                        _selectedNumeratorChannel = null;
                    }
                }
                if (IsInitialized)
                {
                    SetDefaultRange(_isLogScale);
                    UpdateTitle();
                }
                OnPropertyChanged();
                OnPropertyChanged("SelectedNumeratorChannel");
                OnFeatureUpdate();
            }
        }

        private Channel _selectedNumeratorChannel;

        public Channel SelectedNumeratorChannel
        {
            get { return _selectedNumeratorChannel; }
            set
            {
                if (_selectedNumeratorChannel == value)
                {
                    return;
                }
                SetProperty(ref _selectedNumeratorChannel, value);
                UpdateTitle();
                OnFeatureUpdate();
            }
        }

        private Feature _selectedDenominatorFeature;

        public Feature SelectedDenominatorFeature
        {
            get { return _selectedDenominatorFeature; }
            set
            {
                if (_selectedDenominatorFeature == value)
                {
                    return;
                }
                _selectedDenominatorFeature = value;

                if (_selectedDenominatorFeature != null)
                {
                    if (_selectedDenominatorFeature.IsPerChannel)
                    {
                        IsDenominatorChannelEnabled = true;
                        SelectedDenominatorChannel = _denominatorChannelList[0];
                    }
                    else
                    {
                        IsDenominatorChannelEnabled = false;
                        SelectedDenominatorChannel = null;
                    }
                }
                UpdateTitle();
                OnPropertyChanged();
                OnFeatureUpdate();
            }
        }

        private Channel _selectedDenominatorChannel;

        public Channel SelectedDenominatorChannel
        {
            get { return _selectedDenominatorChannel; }
            set
            {
                if (_selectedDenominatorChannel == value)
                {
                    return;
                }

                SetProperty(ref _selectedDenominatorChannel, value);
                UpdateTitle();
                OnFeatureUpdate();
            }
        }

        private string _labelString = string.Empty;

        public string LabelString
        {
            get { return _labelString; }
            set
            {
                if (_labelString == value)
                {
                    return;
                }
                Title = value;
                SetProperty(ref _labelString, value);
            }
        }

        private bool _isNumeratorChannelEnabled;

        public bool IsNumeratorChannelEnabled
        {
            get { return _isNumeratorChannelEnabled; }
            set
            {
                if (_isNumeratorChannelEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isNumeratorChannelEnabled, value);
            }
        }

        private bool _isDenominatorChannelEnabled;

        public bool IsDenominatorChannelEnabled
        {
            get { return _isDenominatorChannelEnabled; }
            set
            {
                if (_isDenominatorChannelEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isDenominatorChannelEnabled, value);
            }
        }

        private bool _isRangeEnabled = true;

        public bool IsRangeEnabled
        {
            get { return _isRangeEnabled; }
            set
            {
                if (_isRangeEnabled == value)
                {
                    return;
                }
                SetProperty(ref _isRangeEnabled, value);
            }
        }

        private ImpObservableCollection<Feature> _numeratorFeatureList = new ImpObservableCollection<Feature>();

        public ImpObservableCollection<Feature> NumeratorFeatureList
        {
            get { return _numeratorFeatureList; }
            set { _numeratorFeatureList = value; }
        }

        private readonly ImpObservableCollection<Channel> _numeratorChannelList = new ImpObservableCollection<Channel>();

        public ImpObservableCollection<Channel> NumeratorChannelList
        {
            get { return _numeratorChannelList; }
        }

        private readonly ImpObservableCollection<Feature> _denominatorFeatureList = new ImpObservableCollection<Feature>();

        public ImpObservableCollection<Feature> DenominatorFeatureList
        {
            get { return _denominatorFeatureList; }
        }

        private readonly ImpObservableCollection<Channel> _denominatorChannelList = new ImpObservableCollection<Channel>();

        public ImpObservableCollection<Channel> DenominatorChannelList
        {
            get { return _denominatorChannelList; }
        }
        #endregion

        #region Constructor

        public AxisModel(AxesEnum type)
        {
            BinCount = ConstantHelper.LowBinCount;
            IsMaxCountChanged = false;
            AxisType = type;
            //  SetTickScale();
        }

        #endregion

        #region Methods

        public void SetRange(double min, double max)
        {
            _minRange = min;
            _maxRange = max;
            _minValue = IsLogScale ? Math.Pow(10, min) : min;
            _maxValue = IsLogScale ? Math.Pow(10, max) : max;
            OnPropertyChanged("MinRange");
            OnPropertyChanged("MaxRange");
            OnPropertyChanged("MinValue");
            OnPropertyChanged("MaxValue");
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public void CalcFeatureIndex()
        {
            if (_selectedNumeratorFeature != null)
            {
                NumeratorFeatureIndex = _selectedNumeratorFeature.Index;
                if (_selectedNumeratorFeature.IsPerChannel)
                {
                    NumeratorFeatureIndex += _selectedNumeratorChannel.ChannelId;
                }
            }

            if (_selectedDenominatorFeature != null)
            {
                DenominatorFeatureIndex = _selectedDenominatorFeature.Index;
                if (_selectedDenominatorFeature.IsPerChannel)
                {
                    DenominatorFeatureIndex += _selectedDenominatorChannel.ChannelId;
                }
            }
        }

        public void AddFeatures(IEnumerable featureList)
        {
            _numeratorFeatureList.Clear();
            _denominatorFeatureList.Clear();

            var noneFeature = new Feature(FeatureType.None);
            _denominatorFeatureList.Add(noneFeature);
            foreach (var feature in featureList)
            {
                var f = (Feature)feature;
                _numeratorFeatureList.Add(f);
                _denominatorFeatureList.Add(f);
            }
        }

        public void AddChannles(IEnumerable channelList)
        {
            _numeratorChannelList.Clear();
            _denominatorChannelList.Clear();
            foreach (var channel in channelList)
            {
                var ch = (Channel)channel;
                _numeratorChannelList.Add(ch);
                _denominatorChannelList.Add(ch);
            }
        }

        public int GetValueBaseOnLog(double value)
        {
            return CalculateLogScale((long)value);
        }

        public bool IsValidate()
        {
            var result = true;
            if (_selectedNumeratorFeature.IsPerChannel)
            {
                if (_selectedNumeratorChannel == null)
                {
                    MessageBox.Show("Select Channel", "ThorCyte", MessageBoxButton.OK);
                    result = false;
                }
            }

            if (_selectedDenominatorFeature != null && _selectedDenominatorFeature.IsPerChannel)
            {
                if (_selectedDenominatorChannel == null)
                {
                    MessageBox.Show("Select Channel", "ThorCyte", MessageBoxButton.OK);
                    result = false;
                }
            }
            return result;
        }

        public float GetFeatureValue(BioEvent ev)
        {
            var v = _selectedNumeratorFeature != null && NumeratorFeatureIndex <= ev.Buffer.Length - 1 ? ev[NumeratorFeatureIndex] : 0.0f;

            if (_selectedDenominatorFeature != null && _selectedDenominatorFeature.FeatureType != FeatureType.None)
            {
                v /= ev[DenominatorFeatureIndex];
            }
            return v;
        }

        public void UpdateTitle()
        {
            if (!_isDefaultLabel)
            {
                Title = _labelString;
                return;
            }
            var temStr = new StringBuilder();
            if (_selectedNumeratorFeature == null)
            {
                Title = string.Empty;
            }
            else
            {
                if (_selectedNumeratorFeature.IsPerChannel)
                    temStr.Append(string.Format("{0} {1}", _selectedNumeratorChannel.ChannelName, _selectedNumeratorFeature.Name));
                else
                    temStr.Append(_selectedNumeratorFeature.Name);

                if (_selectedDenominatorFeature != null && _selectedDenominatorFeature.FeatureType != FeatureType.None)
                {
                    if (_selectedDenominatorFeature.IsPerChannel)
                    {
                        temStr.Append(_selectedNumeratorChannel == null
                            ? string.Format("/{0}", _selectedDenominatorFeature.Name)
                            : string.Format("/{0} {1}", _selectedDenominatorChannel.ChannelName,
                                _selectedDenominatorFeature.Name));
                    }
                    else
                    {
                        temStr.Append(string.Format("/{0}", _selectedDenominatorFeature.Name));
                    }
                }
            }
            Title = temStr.ToString();
        }

        public static float GetFetureDefaultMax(FeatureType featureType, bool logScale)
        {
            switch (featureType)
            {
                case FeatureType.PeripheralIntensity:
                case FeatureType.Intensity:
                case FeatureType.PeripheralMax:
                case FeatureType.MaxPixel:
                case FeatureType.Background:
                    if (logScale)
                        return 4;
                    return 16384;//BioImage.MaxValue + 1;
                case FeatureType.Integral:
                case FeatureType.PeripheralIntegral:
                    return logScale ? 7 : 10000000f;
                case FeatureType.ParentId:
                    return 40f;
                case FeatureType.Perimeter:
                    return logScale ? 2 : 60f;
                case FeatureType.Circularity:
                    return logScale ? 2 : 50f;
                case FeatureType.Time:
                    return logScale ? 4 : 10000f;
                case FeatureType.WellNo:
                    return logScale ? 2f : 100f;
                case FeatureType.Merged:
                    return 2;
                case FeatureType.Diameter:
                    return logScale ? 2f : 20f;
                case FeatureType.Eccentricity:
                    return logScale ? 1f : 2f;
                case FeatureType.Elongation:
                    return logScale ? 1f : 5f;
                case FeatureType.MajorAxis:
                case FeatureType.MinorAxis:
                    return logScale ? 2f : 20f;
                case FeatureType.HalfRadius:
                    return logScale ? 2f : 20f;
                case FeatureType.Stdv:
                    return logScale ? 4f : 10000f;
                case FeatureType.Scan:
                    if (logScale)
                        return 3;
                    return 768;//DaqScan.FieldYPixels;
                default:
                    if (featureType == FeatureType.Area ||
                        featureType.ToString().StartsWith(FeatureType.Area.ToString()))
                        return logScale ? 3 : 800;
                    if (featureType.ToString().StartsWith(FeatureType.Count.ToString()))
                        return logScale ? 2 : 20f;
                    break;
            }

            return logScale ? 2 : 100f;
        }

        public void SetDefaultRange(bool isLogScale)
        {
            if (SelectedNumeratorFeature != null)
            {
                _minRange = 0;
                _maxRange = GetFetureDefaultMax(SelectedNumeratorFeature.FeatureType, isLogScale);
                MinValue = _isLogScale ? Math.Pow(10, 0) : 0;
                MaxValue = _isLogScale ? Math.Pow(10, _maxRange) : _maxRange;
                OnPropertyChanged("MinRange");
                OnPropertyChanged("MaxRange");
            }

            if (isLogScale)
            {
                InitLogTable();
            }
        }

        private void SetOldRange()
        {
            _minRange = OldMinRange;
            _maxRange = OldMaxRange;
            MinValue = _isLogScale ? Math.Pow(10, _minRange) : _minRange;
            MaxValue = _isLogScale ? Math.Pow(10, _maxRange) : _maxRange;
            OnPropertyChanged("MinRange");
            OnPropertyChanged("MaxRange");
        }

        public void InitLogTable()
        {
            if (_logTable.Length != BinCount)
            {
                _logTable = new long[BinCount + 1];
            }
            for (var i = 0; i < _logTable.Length; i++)
            {
                double f = i * (_maxRange - _minRange) / (BinCount - 1);
                f += _minRange;
                f = Math.Pow(10.0, f);
                if (f > long.MaxValue) f = long.MaxValue; // jcl-5331
                _logTable[i] = (long)f;
            }
        }

        private int CalculateLogScale(long nValue)
        {
            if (nValue <= 0) return 0;

            if (nValue > _logTable[BinCount - 1])
                return BinCount;
            var i = BinCount;
            var j = 0;

            do	// This is a binary search
            {
                i = (i + 1) >> 1;
                if (i + j >= BinCount)
                    continue;
                if (_logTable[i + j] <= nValue)
                    j += i;
            } while (i > 1);

            while (j > 0)	// locate the first element if duplicates exist
            {
                if (_logTable[j] == _logTable[j - 1])
                {
                    j--;
                    continue;
                }
                break;
            }

            return j;
        }

        public double GetActualValue(int index)
        {
            return _logTable[index];
        }

        private void OnFeatureUpdate()
        {
            CalcFeatureIndex();
            if (IsInitialized && !IsSwitchWell && !IsMaxCountChanged)
            {
                GraphicModule.GraphicManagerVmInstance.UpdateGraphFeatures(GraphicId);
            }
        }

        #endregion
    }
}
