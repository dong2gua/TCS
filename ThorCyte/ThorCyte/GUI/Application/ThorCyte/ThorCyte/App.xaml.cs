using System.Windows;

namespace ThorCyte
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            var bootstrapper = new Bootstraper();
            bootstrapper.Run();
        }
    }
}
