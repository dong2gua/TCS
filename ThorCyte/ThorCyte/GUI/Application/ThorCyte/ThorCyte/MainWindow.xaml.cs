﻿using System.Windows;
using System.Windows.Controls;
using Prism.Events;
using ThorCyte.Infrastructure.Events;

namespace ThorCyte
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : NoGdiWindow
    {
        private IEventAggregator _eventAggregator;

        public MainWindow(IEventAggregator eventAggregator)
        {   
            InitializeComponent();
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<ExperimentLoadedEvent>().Subscribe(ExperimentLoaded);
        }

        private void ExperimentLoaded(int obj)
        {
            RightCol.SetValue(ColumnDefinition.WidthProperty, new GridLength(325));
        }
    }
}
