using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Events;
using ThorCyte.Infrastructure.Events;

namespace MosicaModule.Views
{
    /// <summary>
    /// Interaction logic for MosicaView.xaml
    /// </summary>
    public partial class MosicaView : UserControl
    {
        private bool _isSelectChanged;
        private IEventAggregator _eventAggregator;
        public MosicaView(IEventAggregator eventAggregator)
        {
            InitializeComponent();
            _isSelectChanged = false;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<MacroFinishEvent>().Subscribe(SwithView, ThreadOption.UIThread, true);
        }

        private void SwithView(int obj)
        {
            TabControl.SelectedIndex = 2;
        }

        private void OnSelectTabChanged(object sender, SelectionChangedEventArgs e)
        {
            TabControl tabControl = sender as TabControl;
            if (tabControl.SelectedIndex == 0)
            {
                if (_isSelectChanged)
                {
                    _eventAggregator.GetEvent<ShowRegionEvent>().Publish("ReviewModule");
                    _isSelectChanged = false;
                }

            }
            else
            {
                if (!_isSelectChanged)
                {
                    _eventAggregator.GetEvent<ShowRegionEvent>().Publish("AnalysisModule");
                    _isSelectChanged = true; 
                }
                
            }
        }
    }
}
