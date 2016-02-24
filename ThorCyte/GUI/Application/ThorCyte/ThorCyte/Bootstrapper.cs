using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using ThorCyte.Infrastructure.Interfaces;

namespace ThorCyte
{
    public class Bootstraper : UnityBootstrapper
    {
        // TODO: 02 - The Shell loads the EmployeeModule, as specified in the module catalog (ModuleCatalog.xaml).

        protected override IModuleCatalog CreateModuleCatalog()
        {
            return new DirectoryModuleCatalog() { ModulePath = "DirectoryModules" };
        }

        protected override DependencyObject CreateShell()
        {
            var view = this.Container.TryResolve<MainWindow>();
            var _log = new ThorCyteLog();
            Container.RegisterInstance<ILog>(_log);
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
