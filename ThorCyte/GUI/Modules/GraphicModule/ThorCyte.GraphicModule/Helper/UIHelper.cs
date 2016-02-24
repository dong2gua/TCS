using System;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;

namespace ThorCyte.GraphicModule.Helper
{
    public static class UIHelper
    {
        public static void OnCheckTabNameFailed(string name)
        {
            var action = new Action(() => MessageBox.Show(string.Format("The tab name {0} has exist.",name),"Message"));

            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(action);
            }
        }
    }
}