using System.Windows;
using Abt.Controls.SciChart.Visuals;

namespace ThorCyte
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            SciChartSurface.SetRuntimeLicenseKey(@"<LicenseContract>
            <Customer>Thorlabs</Customer>
            <OrderId>ABT150916-7903-84104</OrderId>
            <LicenseCount>1</LicenseCount>
            <IsTrialLicense>false</IsTrialLicense>
            <SupportExpires>01/24/2016 00:00:00</SupportExpires>
            <ProductCode>SC-WPF-PRO</ProductCode>
            <KeyCode>lwAAAAEAAADkBWp2vvDQAWUAQ3VzdG9tZXI9VGhvcmxhYnM7T3JkZXJJZD1BQlQxNTA5MTYtNzkwMy04NDEwNDtTdWJzY3JpcHRpb25WYWxpZFRvPTI0LUphbi0yMDE2O1Byb2R1Y3RDb2RlPVNDLVdQRi1QUk99dyKgCYuoNoXOUhV2ki594OYNW3lLPWgUFOImlnAvh1jHrW+N0AThdTBNfZJkhIA=</KeyCode>
               </LicenseContract>");
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var bootstrapper = new Bootstraper();
            bootstrapper.Run();
        }
    }
}
