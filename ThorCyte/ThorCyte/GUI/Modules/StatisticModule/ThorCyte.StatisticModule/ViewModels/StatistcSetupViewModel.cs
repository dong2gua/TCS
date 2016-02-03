using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Prism.Interactivity.InteractionRequest;
using ThorCyte.Statistic.Models;
using Prism.Events;
using Microsoft.Practices.Unity;
using ThorCyte.Infrastructure.Interfaces;
using System.Linq;
using System.Collections.Generic;
using ThorCyte.Statistic.Views;

namespace ThorCyte.Statistic.ViewModels
{
    public class StatisticSetupViewModel : BindableBase
    {
        //private StatisticDataNotification notification;
        private StatisticModel ModelAdapter { get; set; }
        private IPopupDetailWindow PopupWindowAdapter { get; set; }
        private IPopupSetupWindow PopupSetupAdapter { get; set; }
        public StatisticSetupViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment, StatisticModel model, IPopupDetailWindow popupwin, IPopupSetupWindow pp)
        {
            SetupPopupDetail = new InteractionRequest<StatisticDataNotification>();
            ModelAdapter = model;
            SelectedComponent = ModelAdapter.SelectedComponent;
            PopupWindowAdapter = popupwin;
            PopupSetupAdapter = pp;
            TabComponent = ModelAdapter.ComponentContainer;
            SelectedComponent = ModelAdapter.SelectedComponent;
            OnPropertyChanged(() => TabComponent);
            OnPropertyChanged(() => SelectedComponent);
        }

        //public Action FinishInteraction { get; set; }

        ////Receive Notification
        //public INotification Notification
        //{
        //    get
        //    {
        //        return this.notification;
        //    }
        //    set
        //    {
        //        if(value is StatisticDataNotification)
        //        {
        //            this.notification = value as StatisticDataNotification;
        //            var model = this.notification.StatisticModelPot;
        //            //make tab source
        //            TabComponent = model.ComponentContainer;
        //            this.OnPropertyChanged(() => TabComponent);
        //        }
        //    }
        //}

        public List<ComponentRunFeature> TabComponent { get; set; }

        #region CurrentTab
        private ComponentRunFeature _SelectedComponent;

        public ComponentRunFeature SelectedComponent
        {
            get { return _SelectedComponent; }
            set {
                if (value != null)
                {
                    //to do: get runfeature from hard disk
                    _SelectedComponent = value;
                    OnPropertyChanged(() => SelectedRunFeature);
                }
            }

        }
        #endregion

        private RunFeature _SelectedRunFeature;

        public RunFeature SelectedRunFeature
        {
            get { return _SelectedRunFeature; }
            set {
                _SelectedRunFeature = value; }
        }

        public InteractionRequest<StatisticDataNotification> SetupPopupDetail { get; private set; }
        public ICommand SetupStatisticDetailCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (SelectedRunFeature != null)
                    {
                        ModelAdapter.SelectedRunFeature = SelectedRunFeature;
                    }
                    PopupWindowAdapter.PopupWindow();
                    PopupSetupAdapter.Close();
                }, () => true);
            }
        }

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    PopupSetupAdapter.Close();
                });
            }
        }
    }
}
