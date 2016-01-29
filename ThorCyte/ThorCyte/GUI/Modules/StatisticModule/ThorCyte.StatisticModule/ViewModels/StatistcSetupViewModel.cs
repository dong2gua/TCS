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

namespace ThorCyte.Statistic.ViewModels
{
    public class StatisticSetupViewModel : BindableBase, IInteractionRequestAware
    {
        private StatisticDataNotification notification; 
        public StatisticSetupViewModel(IEventAggregator eventAggregator, IUnityContainer container, IExperiment experiment)
        {
            SetupPopupDetail = new InteractionRequest<StatisticDataNotification>();
            TabComponent = new List<ComponentRunFeature>();
            _SelectedComponent = null;
            _SelectedRunFeature = null;
        }

        public Action FinishInteraction { get; set; }

        //Receive Notification
        public INotification Notification
        {
            get
            {
                return this.notification;
            }
            set
            {
                if(value is StatisticDataNotification)
                {
                    this.notification = value as StatisticDataNotification;
                    var model = this.notification.StatisticModelPot;
                    //make tab source
                    TabComponent = model.ComponentContainer.Select(x =>
                        {
                            return new ComponentRunFeature()
                            {
                                CurrentComponent = x,
                                RunFeatureContainer = model.RunFeatureContainer.Where(y => y.ComponentContainer.Contains(x)).ToList()
                            };
                        }).ToList();
                    this.OnPropertyChanged(() => TabComponent);
                }
            }
        }

        public List<ComponentRunFeature> TabComponent { get; set; }

        #region CurrentTab
        private ComponentRunFeature _SelectedComponent;

        public ComponentRunFeature SelectedComponent
        {
            get { return _SelectedComponent; }
            set {
                if (value != null)
                {
                    this.notification.SelectedComponent = value.CurrentComponent;
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
                this.notification.SelectedRunFeature = value;
                _SelectedRunFeature = value; }
        }

        public InteractionRequest<StatisticDataNotification> SetupPopupDetail { get; private set; }
        public ICommand SetupStatisticDetailCommand
        {
            get
            {
                return new DelegateCommand(() => {
                    SetupPopupDetail.Raise(notification, (result) => { FinishInteraction(); });
                }, () => true);
            }
        }

        public ICommand ConfirmCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    notification.Confirmed = true;
                    FinishInteraction();
                });
            }
        }
    }
}
