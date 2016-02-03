using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using Microsoft.Practices.Unity;
using ThorCyte.Statistic.Models;
using ThorCyte.Statistic.ViewModels;

namespace ThorCyte.Statistic.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class StatisticView : UserControl
    {
        public StatisticView(StatisticViewModel pVM, IUnityContainer container)
        {
            InitializeComponent();
            DataContext = pVM;
        }

    }
    public interface IPopupSetupWindow
    {
        bool PopupWindow();
        bool Close();
    }

    public class PopupSetupWindow : IPopupSetupWindow
    {
        private StatisticSetup _subwin;
        public bool PopupWindow()
        {
            _subwin = new StatisticSetup();
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
