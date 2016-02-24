
using System;
using System.Windows;
using ThorCyte.HeaderModule.Common;

namespace ThorCyte.HeaderModule.Views
{
    /// <summary>
    /// Interaction logic for SaveWaiting.xaml
    /// </summary>
    public partial class SaveWaiting : Window
    {
        public Object TaskResult { get { return m_taskResult; } }

        private Object m_taskResult;
        private bool m_bCloseByMe;
        private readonly ILongTimeTask m_task;
        private readonly string m_strThePromptText;

        public SaveWaiting(ILongTimeTask task, string strThePromptText = null)
        {
            m_task = task;
            m_strThePromptText = strThePromptText;
            InitializeComponent();
        }
        private delegate void CloseMethod();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(m_strThePromptText))
            {
                //tbPrompt.Text = m_strThePromptText;
            }
            m_task.Start(this);
        }

        public void TaskEnd(Object result)
        {
            m_taskResult = result;
            m_bCloseByMe = true;
            Dispatcher.BeginInvoke(new CloseMethod(Close));
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!m_bCloseByMe) { e.Cancel = true; }
        }
    }
}
