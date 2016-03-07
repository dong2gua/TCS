using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ComponentDataService;
using ComponentDataService.Types;
using ROIService;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.GraphicModule.Views
{
    /// <summary>
    /// Interaction logic for RegionEventsViewer.xaml
    /// </summary>
    public partial class RegionEventsViewer
    {
        private const int Limit = 500;

        private readonly List<Feature> _features;
        private IList<Channel> _channels;
        private readonly DataTable _dataTable;

        public RegionEventsViewer()
        {
            InitializeComponent();
            _dataTable = new DataTable();
            _dataTable.Columns.Add(new DataColumn("Channel", typeof(string)));   
            _features = new List<Feature>();
        }

        public void Update(string graphicName,string component, IList<string> regionIds)
        {
            if (string.IsNullOrEmpty(component) || regionIds.Count == 0)
            {
                return;
            }
            var features = ComponentDataManager.Instance.GetFeatures(component);
            if (features.Count > 0)
            {
                var idFeature = features.FirstOrDefault(f => f.FeatureType == FeatureType.Id);
                if (idFeature != null)
                {
                    _features.Add(idFeature);
                }
                var wellNoFeature = features.FirstOrDefault(f => f.FeatureType == FeatureType.WellNo);
                if (wellNoFeature != null)
                {
                    _features.Add(wellNoFeature);
                }
                foreach (var f in features)
                {
                    if (f.FeatureType != FeatureType.Id && f.FeatureType != FeatureType.WellNo)
                    {
                        _features.Add(f);
                    }
                }
            }
            _channels = ComponentDataManager.Instance.GetChannels(component);
            var count = _channels.Count;
            var events = new List<BioEvent>();

            foreach (var id in regionIds)
            {
                var list = ROIManager.Instance.GetEvents(id);
                events.AddRange(list);
            }
            
            foreach (var f in _features)
            {
                if (f.IsPerChannel)
                {
                    for (var i = 0; i < count; i++)
                    {
                        var header = _channels[i].ChannelName + " " + f.Name;
                        var index = f.Index + i;
                        var bindPath = string.Format("Buffer[{0}]", index);
                        EventInfoList.Columns.Add(new DataGridTextColumn { Header = header,  Binding = new Binding(bindPath) });
                        if (i == 0)
                        {
                            _dataTable.Columns.Add( new DataColumn(f.Name, typeof(string)));
                        }
                    }
                }
                else
                {
                    var header = f.Name;
                    var index = f.Index;
                    var bindPath = string.Format("Buffer[{0}]", index);
                    var headerBinding = new Binding(bindPath);
                    EventInfoList.Columns.Add(new DataGridTextColumn { Header = header, Binding = headerBinding });
                    DetailEvent.Columns.Add(new DataGridTextColumn { Header = header, Binding = headerBinding });
                }
            }
           
            events = events.Take(Limit).ToList();
            Title = GetTitle(graphicName, component, regionIds, events.Count);
            if (events.Count == 0)
            {
                BusyIndicator.Visibility = Visibility.Collapsed;
                return;
            }
            EventInfoList.ItemsSource = events;
            BusyIndicator.Visibility = Visibility.Collapsed;
            DetailPerEvent.DataContext = _dataTable;
        }

        private string GetTitle(string graphicName,string component, IEnumerable<string> regionIds,int count)
        {
            var title = new StringBuilder(string.Format("[{0}:", graphicName));
            foreach (var id in regionIds)
            {
                title.Append(string.Format("{0},",id));
            }
            title.Remove(title.Length - 1,1);
            title.Append(string.Format("] {0}", component));
            title.Append(string.Format("(Count:{0})", count));
            return title.ToString();
        }

        private void OnSelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EventInfoList.SelectedItem == null)
            {
                return;
            }
            DetailEvent.ItemsSource = new List<object> { EventInfoList.SelectedItem };

            _dataTable.Rows.Clear();
            var currentEvent = EventInfoList.SelectedItem as BioEvent;
            if (currentEvent == null)
            {
                return;
            }
            for (var i = 0 ;i< _channels.Count;i++)
            {
                var row = _dataTable.NewRow();
                _dataTable.Rows.Add(row);
                row[0] = _channels[i].ChannelName;
                foreach (var feature in _features)
                {
                    if (feature.IsPerChannel)
                    {
                        row[feature.Name] = currentEvent.Buffer[feature.Index+i];
                    }
                }
            }
        }
    }
}
