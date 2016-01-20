using Abt.Controls.SciChart.Visuals;
using Prism.Modularity;
using Prism.Regions;
using ThorCyte.GraphicModule.ViewModels;
using ThorCyte.GraphicModule.Views;
using ThorCyte.Infrastructure.Commom;

namespace ThorCyte.GraphicModule
{
    public class GraphicModule : IModule
    {
        private readonly IRegionViewRegistry _regionViewRegistry;

        public static GraphicManagerVm GraphicManagerVmInstance
        {
           get { return _graphicManagerVmInstance; } 
        }

        private static GraphicManagerVm _graphicManagerVmInstance;

        public GraphicModule(IRegionViewRegistry regionViewRegistry)
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
            _regionViewRegistry = regionViewRegistry;
        }

        public void Initialize()
        {
            _regionViewRegistry.RegisterViewWithRegion(RegionNames.GraphicRegion, typeof(GraphicModeuleView));
        }

        public static void RegisterGaphicManager(GraphicManagerVm graphicManagerVm)
        {
            _graphicManagerVmInstance = graphicManagerVm;
        }
    }
}