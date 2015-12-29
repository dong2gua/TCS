using System;
using System.Windows.Input;

namespace ThorCyte.CarrierModule.Common
{
    public class WaitCursor : IDisposable
    {
        private readonly Cursor _previousCursor;

        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
        #endregion
    }
}
