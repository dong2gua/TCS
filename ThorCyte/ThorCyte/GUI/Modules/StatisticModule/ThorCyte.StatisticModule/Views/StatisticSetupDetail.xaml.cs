

namespace ThorCyte.Statistic.Views
{
    /// <summary>
    /// Interaction logic for StatisticsSetupDetail.xaml
    /// </summary>
    public partial class StatisticSetupDetail
    {
        public StatisticSetupDetail()
        {
            InitializeComponent();
        }
    }
    
    public interface IPopupDetailWindow
    {
        bool PopupWindow();
        bool Close();
    }

    public class PopupDetailWindow : IPopupDetailWindow
    {
        private StatisticSetupDetail _subwin;
        public bool PopupWindow()
        {
            _subwin = new StatisticSetupDetail();
            return _subwin.ShowDialog() ?? false;
        }

        public bool Close()
        {
            if (_subwin != null)
            {
                _subwin.DialogResult = true;
                _subwin.Close();
                _subwin = null;
            }
            return true;
        }
    }
}
