using System;
using System.Collections;

namespace ThorCyte.GraphicModule.Infrastructure
{
    /// <summary>
    /// Arguments to the ItemsAdded and ItemsRemoved events.
    /// </summary>
    public class CollectionItemsChangedEventArgs : EventArgs
    {
        #region  Properties and Fields

        private readonly ICollection _items;

        /// <summary>
        /// The collection of _items that changed.
        /// </summary>
        public ICollection Items
        {
            get
            {
                return _items;
            }
        }

        #endregion

        #region Methods

        public CollectionItemsChangedEventArgs(ICollection items)
        {
            _items = items;
        }

        #endregion
    }
}
