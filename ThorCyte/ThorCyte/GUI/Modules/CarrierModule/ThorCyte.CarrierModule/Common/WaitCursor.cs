using System;
using System.Windows.Input;

namespace ThorCyte.CarrierModule.Common
{
    public class WaitCursor : IDisposable
    {
        #region IDisposable Fields
        private readonly Cursor _previousCursor;
        #endregion

        #region IDisposable Constructor
        public WaitCursor()
        {
            _previousCursor = Mouse.OverrideCursor;

            Mouse.OverrideCursor = Cursors.Wait;
        }
        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            Mouse.OverrideCursor = _previousCursor;
        }
        #endregion
    }
}
