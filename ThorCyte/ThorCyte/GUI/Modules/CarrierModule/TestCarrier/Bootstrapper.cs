﻿using System.Windows;
using Prism.Modularity;
using Prism.Unity;
using ThorCyte;

namespace TestCarrier
{
    public class Bootstraper : UnityBootstrapper
    {
        // TODO: 02 - The Shell loads the EmployeeModule, as specified in the module catalog (ModuleCatalog.xaml).

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = @"..\..\..\ThorCyte.CarrierModule\bin\Debug" };
        }

        protected override DependencyObject CreateShell()
        {
            var view = this.Container.TryResolve<MainWindow>();
            return view;
        }

        protected override void InitializeShell()
        {
            base.InitializeShell();
            App.Current.MainWindow = (Window)this.Shell;
            App.Current.MainWindow.Show();
        }
    }
}
