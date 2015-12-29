using Prism.Mvvm;
using ThorCyte.Infrastructure.Types;
using ThorCyte.ProtocolModule.Utils;

namespace ThorCyte.ProtocolModule.Models
{
    public class ChannelModel : BindableBase
    {
        #region Properties and Fields

        private Channel _selctedChannel;

        public Channel SelectedChannel
        {
            get { return _selctedChannel; }
            set { SetProperty(ref _selctedChannel, value); }
        }

        ImpObservableCollection<string> _channels = new ImpObservableCollection<string>();

        public ImpObservableCollection<string> Channels
        {
            get { return _channels; }
            set { _channels = value; }
        }

        #endregion
    }
}
