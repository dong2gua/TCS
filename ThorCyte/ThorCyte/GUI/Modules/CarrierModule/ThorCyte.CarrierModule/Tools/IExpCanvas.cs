using System.Windows;

namespace ThorCyte.CarrierModule.Tools
{
    interface IExpCanvas
    {
        void SetActiveRegions();
        void UpdateScanArea();
        Point ClientToWorld();

    }
}
