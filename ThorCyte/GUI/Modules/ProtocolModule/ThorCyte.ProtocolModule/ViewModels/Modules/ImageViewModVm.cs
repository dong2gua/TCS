using System;
using System.Diagnostics;
using System.Xml;
using ImageProcess;
using Microsoft.Practices.ServiceLocation;
using Prism.Events;
using ThorCyte.ProtocolModule.Events;
using ThorCyte.ProtocolModule.Models;

namespace ThorCyte.ProtocolModule.ViewModels.Modules
{
    public class ImageViewModVm : ModuleBase
    {
        public override bool Executable
        {
            get { return _imageTitle != string.Empty; }
        }

        public override string CaptionString
        {
            get
            {
                return string.Format("{0}", _imageTitle);
            }
        }

        private string _imageTitle = string.Empty;
        public string ImageTitle
        {
            get { return _imageTitle; }
            set
            {
                if (_imageTitle == value) return;
                SetProperty(ref _imageTitle, value);
                OnPropertyChanged("CaptionString");
            }
        }

        private static IEventAggregator _eventAggregator;
        private static IEventAggregator EventAggregator
        {
            get
            {
                return _eventAggregator ?? (_eventAggregator = ServiceLocator.Current.GetInstance<IEventAggregator>());
            }
        }

        private ImageData _img1;
        public override void OnExecute()
        {
            try
            {
                _img1 = GetInPort(0).Image;
                //do something

                View.Dispatcher.Invoke(() =>
                {
                    var imgs = _img1.ToBitmapSource();
                    var arg = new DisplayImageEventArgs(_imageTitle, imgs);
                    EventAggregator.GetEvent<DisplayImageEvent>().Publish(arg);
                });

                _img1.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Add Module error: " + ex.Message);
                throw;
            }
        }

        public override void Initialize()
        {
            ModType = ModuleType.SmtImageViewModule;
            HasImage = true;
            InputPorts[0].DataType = PortDataType.Image;
            InputPorts[0].ParentModule = this;
        }

        public override void OnDeserialize(XmlReader reader)
        {
            _imageTitle = reader["title"];
        }

        public override void OnSerialize(XmlWriter writer)
        {
            writer.WriteAttributeString("title", _imageTitle);
        }
    }
}
