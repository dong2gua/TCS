using ImageProcess;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ThorCyte.ImageViewerModule.Viewmodel;
using ThorCyte.Infrastructure.Events;
using ThorCyte.Infrastructure.Interfaces;
using ThorCyte.Infrastructure.Types;

namespace ThorCyte.ImageViewerModule.Model
{
    public class ChannelImage : BindableBase
    {
        public ChannelImage(IData iData, int scanId,   int maxBits,double aspectRatio, ViewportViewModel viewport)
        {
            _iData = iData;
            _scanId = scanId;
            _maxBits = maxBits;
            _aspectRatio = aspectRatio;
            _viewport = viewport;
            Contrast = 1;
            Brightness = 0;
            IsComputeColor = false;
        }
        public Channel ChannelInfo { get; set; }
        public ComputeColor ComputeColorInfo { get; set; }
        public string ChannelName { get; set; }
        public ImageSource Image
        {
            get { return _image; }
            set { SetProperty<ImageSource>(ref _image, value, "Image"); }
        }
        public Int32Rect ImageRect
        {
            get { return _imageRect; }
            set { SetProperty<Int32Rect>(ref _imageRect, value, "ImageRect"); }
        }
        public BitmapSource Thumbnail
        {
            get { return _thumbnail; }
            set { SetProperty<BitmapSource>(ref _thumbnail, value, "Thumbnail"); }
        }
        public ImageData ImageData { get; set; }
        public Int32Rect DataRect { get; set; }
        public double DataScale { get; set; }
        public ImageData ThumbnailImageData { get; set; }
        public int Brightness { get; set; }
        public double Contrast { get; set; }
        public bool IsComputeColor { get; set; }
        public Dictionary<Channel, Color> ComputeColorDic { get; set; }
        public Dictionary<Channel, Tuple<ImageData, Color>> ComputeColorDicWithData { get; set; }
        private ViewportViewModel _viewport;
        private ImageSource _image;
        private BitmapSource _thumbnail;
        private Int32Rect _imageRect;
        private int _scanId;
        private IData _iData;
        private int _maxBits;
        private double _aspectRatio;
        private const int ThumbnailSize = 80;
        private int _regionId;
        private int _tileId;
        private FrameIndex _frameId = new FrameIndex() { StreamId = 1, TimeId = 1, ThirdStepId = 1 };
        public void SetRegionTileFrame(int regionId, int tileId, FrameIndex frameId)
        {
            _regionId = regionId;
            _tileId = tileId;
            _frameId = frameId;
        }
        public void UpdateBitmap()
        {
            ImageData result = null;
            if (Contrast == 1 && Brightness == 0)
                result = ImageData;
            else
                result = ImageData.SetBrightnessAndContrast(Contrast, Brightness, _maxBits);
            if (result != null)
                Application.Current.Dispatcher.Invoke(() => { Image = result.ToBitmapSource(_maxBits); });
        }
        public void UpdateThumbnail()
        {
            if (ThumbnailImageData == null) return;
            ImageData result = null;
            if (Contrast == 1 && Brightness == 0)
                result = ThumbnailImageData;
            else
                result = ThumbnailImageData.SetBrightnessAndContrast(Contrast, Brightness, _maxBits);
            if (result != null)
                Application.Current.Dispatcher.Invoke(() => { Thumbnail = result.ToBitmapSource(_aspectRatio, _maxBits); });
        }
        public async Task GetDataAsync( Int32Rect rect, double scale)
        {
            if (ImageData != null)
            {
                ImageData.Dispose();
                ImageData = null;
            }
            if (IsComputeColor)
            {
                ComputeColorDicWithData = await getComputeDictionaryAsync(ComputeColorDic, rect, scale);
                ImageData = getComputeData(ComputeColorDicWithData, rect, scale);

            }
            else if (ChannelInfo.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                await TraverseVirtualChannelAsync(dic, ChannelInfo as VirtualChannel, rect, scale);
                ImageData = getVirtualData(ChannelInfo as VirtualChannel, dic);
            }
            else
            {
                ImageData = await Task.Run<ImageData>(()=> getActiveData(ChannelInfo, rect, scale));
            }
            DataScale = scale;
            DataRect = rect;
        }
        public void GetData(Int32Rect rect, double scale)
        {
            if (ImageData != null)
            {
                ImageData.Dispose();
                ImageData = null;
            }
            if (IsComputeColor)
            {
                ComputeColorDicWithData = getComputeDictionary(ComputeColorDic, rect, scale);
                ImageData = getComputeData(ComputeColorDicWithData, rect, scale);

            }
            else if (ChannelInfo.IsvirtualChannel)
            {
                var dic = new Dictionary<Channel, ImageData>();
                TraverseVirtualChannel(dic, ChannelInfo as VirtualChannel, rect, scale);
                ImageData = getVirtualData(ChannelInfo as VirtualChannel, dic);
            }
            else
            {
                ImageData = getActiveData(ChannelInfo, rect, scale);
            }
            DataScale = scale;
            DataRect = rect;
        }
        public async Task GetThumbnailDataAsync()
        {
            var rect = new Int32Rect(0, 0, (int)_viewport.ImageSize.Width, (int)_viewport.ImageSize.Height);
            var scale = Math.Min(ThumbnailSize / _viewport.ImageSize.Width, ThumbnailSize / _viewport.ImageSize.Height);
            var tileThumbnailDic = new Dictionary<Channel, ImageData>();
            if (IsComputeColor)
            {
                if (!_viewport.ThumbnailDic.ContainsKey(_regionId) && _tileId < 0) return;
                ThumbnailImageData = await getThumbnailComputeDataAsync(_tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId], ComputeColorDic, rect, scale);
            }
            else if (ChannelInfo.IsvirtualChannel)
            {
                if (!_viewport.ThumbnailDic.ContainsKey(_regionId) && _tileId < 0) return;
                await TraverseThumbnailVirtualChannelAsync(_tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId], ChannelInfo as VirtualChannel, rect, scale);
                ThumbnailImageData = getVirtualData(ChannelInfo as VirtualChannel, _tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId]);
            }
            else
            {
                ThumbnailImageData = await getThumbnailActiveDataAsync(ChannelInfo, rect, scale);
            }
        }
        public void GetThumbnailData()
        {
            var rect = new Int32Rect(0, 0, (int)_viewport.ImageSize.Width, (int)_viewport.ImageSize.Height);
            var scale = Math.Min(ThumbnailSize / _viewport.ImageSize.Width, ThumbnailSize / _viewport.ImageSize.Height);
            var  tileThumbnailDic = new Dictionary<Channel, ImageData>();
            if (IsComputeColor)
            {
                if (!_viewport.ThumbnailDic.ContainsKey(_regionId) && _tileId < 0) return;
                ThumbnailImageData = getThumbnailComputeData(_tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId], ComputeColorDic, rect, scale);
            }
            else if (ChannelInfo.IsvirtualChannel)
            {
                if (!_viewport.ThumbnailDic.ContainsKey(_regionId) && _tileId < 0) return;
                TraverseThumbnailVirtualChannel( _tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId], ChannelInfo as VirtualChannel, rect, scale);
                ThumbnailImageData = getVirtualData(ChannelInfo as VirtualChannel, _tileId > 0 ? tileThumbnailDic : _viewport.ThumbnailDic[_regionId]);
            }
            else
            {
                ThumbnailImageData = getThumbnailActiveData( ChannelInfo, rect, scale);
            }
        }
        private async Task TraverseVirtualChannelAsync(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                await TraverseVirtualChannelAsync(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, await Task.Run<ImageData>(() =>  getActiveData(channel.FirstChannel, rect, scale)));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    await TraverseVirtualChannelAsync(dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, await Task.Run<ImageData>(() => getActiveData(channel.SecondChannel, rect, scale)));
                }
            }
        }
        private void TraverseVirtualChannel( Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel,  getActiveData(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel( dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, getActiveData( channel.SecondChannel, rect, scale));
                }
            }
        }
        private async Task<Dictionary<Channel, Tuple<ImageData, Color>>> getComputeDictionaryAsync(Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var datadic = new Dictionary<Channel, ImageData>();
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    await TraverseVirtualChannelAsync( datadic, o.Key as VirtualChannel, rect, scale);
                else
                if (!datadic.ContainsKey(o.Key))
                    datadic.Add(o.Key, await Task.Run<ImageData>(() => getActiveData( o.Key, rect, scale)));
            }

            var dic = new Dictionary<Channel, Tuple<ImageData, Color>>();
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                {
                    data = datadic[o.Key];
                }
                else
                {
                    data = getVirtualData(o.Key as VirtualChannel, datadic);
                }
                dic.Add(o.Key, new Tuple<ImageData, Color>(data, o.Value));
            }
            return dic;
        }
        private Dictionary<Channel, Tuple<ImageData, Color>> getComputeDictionary(Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var datadic = new Dictionary<Channel, ImageData>();
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    TraverseVirtualChannel(datadic, o.Key as VirtualChannel, rect, scale);
                else
                if (!datadic.ContainsKey(o.Key))
                    datadic.Add(o.Key, getActiveData( o.Key, rect, scale));
            }

            var dic = new Dictionary<Channel, Tuple<ImageData, Color>>();
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                {
                    data = datadic[o.Key];
                }
                else
                {
                    data = getVirtualData(o.Key as VirtualChannel, datadic);
                }
                dic.Add(o.Key, new Tuple<ImageData, Color>(data, o.Value));
            }
            return dic;
        }
        private async Task TraverseThumbnailVirtualChannelAsync(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                await TraverseVirtualChannelAsync(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, await getThumbnailActiveDataAsync(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    await TraverseVirtualChannelAsync( dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, await getThumbnailActiveDataAsync(channel.SecondChannel, rect, scale));
                }
            }
        }
        private void TraverseThumbnailVirtualChannel(Dictionary<Channel, ImageData> dic, VirtualChannel channel, Int32Rect rect, double scale)
        {
            if (channel.FirstChannel.IsvirtualChannel)
                TraverseVirtualChannel(dic, channel.FirstChannel as VirtualChannel, rect, scale);
            else
            {
                if (!dic.ContainsKey(channel.FirstChannel))
                    dic.Add(channel.FirstChannel, getThumbnailActiveData(channel.FirstChannel, rect, scale));
            }
            if (channel.SecondChannel != null)
            {
                if (channel.SecondChannel.IsvirtualChannel)
                    TraverseVirtualChannel(dic, channel.SecondChannel as VirtualChannel, rect, scale);
                else
                {
                    if (!dic.ContainsKey(channel.SecondChannel))
                        dic.Add(channel.SecondChannel, getThumbnailActiveData(channel.SecondChannel, rect, scale));
                }
            }
        }
        private async Task<ImageData> getThumbnailComputeDataAsync(Dictionary<Channel, ImageData> dic, Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var tData = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    await TraverseThumbnailVirtualChannelAsync(dic, o.Key as VirtualChannel, rect, scale);
                else
                if (!dic.ContainsKey(o.Key))
                    dic.Add(o.Key, await Task.Run<ImageData>(() => getActiveData(o.Key, rect, scale)));
            }
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                    data = dic[o.Key];
                else
                    data = getVirtualData(o.Key as VirtualChannel, dic);
                var d = ToComputeColor(data, o.Value);
                tData = tData.Add( d, _maxBits);
                d.Dispose();
            }
            return tData;
        }
        private ImageData getThumbnailComputeData(Dictionary<Channel, ImageData> dic, Dictionary<Channel, Color> ComputeColorDictionary, Int32Rect rect, double scale)
        {
            var tData = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in ComputeColorDictionary)
            {
                if (o.Key.IsvirtualChannel)
                    TraverseThumbnailVirtualChannel(dic, o.Key as VirtualChannel, rect, scale);
                else
                if (!dic.ContainsKey(o.Key))
                    dic.Add(o.Key,  getActiveData(o.Key, rect, scale));
            }
            foreach (var o in ComputeColorDictionary)
            {
                ImageData data;
                if (!o.Key.IsvirtualChannel)
                    data = dic[o.Key];
                else
                    data = getVirtualData(o.Key as VirtualChannel, dic);
                var d =  ToComputeColor(data, o.Value);
                tData =  tData.Add( d, _maxBits);
                d.Dispose();
            }
            return tData;
        }
        private async Task<ImageData> getThumbnailActiveDataAsync(Channel channelInfo, Int32Rect rect, double scale)
        {
            ImageData tData;
            if (_tileId>0)
                return await Task.Run<ImageData>(() => { return getActiveData(channelInfo, rect, scale); });
            if (_viewport.ThumbnailDic.ContainsKey(_regionId))
            {
                if (_viewport.ThumbnailDic[_regionId].ContainsKey(channelInfo))
                {
                    tData = _viewport.ThumbnailDic[_regionId][channelInfo];
                }
                else
                {
                    tData = await Task.Run<ImageData>(() => getActiveData(channelInfo, rect, scale));
                    _viewport.ThumbnailDic[_regionId].Add( channelInfo, tData);
                }
            }
            else
            {
                var dic = new Dictionary<Channel, ImageData>();
                _viewport.ThumbnailDic.Add( _regionId, dic);
                tData = await Task.Run<ImageData>(() => getActiveData(channelInfo, rect, scale));
                dic.Add( channelInfo, tData);
            }
            return tData;
        }
        private ImageData getThumbnailActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            ImageData tData;
            if (_tileId > 0)
                return  getActiveData(channelInfo, rect, scale);
            if (_viewport.ThumbnailDic.ContainsKey(_regionId))
            {
                if (_viewport.ThumbnailDic[_regionId].ContainsKey(channelInfo))
                {
                    tData = _viewport.ThumbnailDic[_regionId][channelInfo];
                }
                else
                {
                    tData = getActiveData(channelInfo, rect, scale);
                    _viewport.ThumbnailDic[_regionId].Add(channelInfo, tData);
                }
            }
            else
            {
                var dic = new Dictionary<Channel, ImageData>();
                _viewport.ThumbnailDic.Add(_regionId, dic);
                tData = getActiveData(channelInfo, rect, scale);
                dic.Add(channelInfo, tData);
            }
            return tData;
        }
        private ImageData getActiveData(Channel channelInfo, Int32Rect rect, double scale)
        {
            ImageData data = null;
            Application.Current.Dispatcher.Invoke(() => Console.WriteLine(string.Format("r-{0} t-{1} s-{2} c-{3}++++++++++++++++++", _regionId, _tileId, _frameId.StreamId, channelInfo.ChannelId)));
            if (_tileId > 0)
                data = _iData.GetTileData(_scanId, _regionId, channelInfo.ChannelId, _frameId.StreamId, _tileId, _frameId.TimeId).Resize((int)(rect.Width * scale), (int)(rect.Height * scale));
            else
                data = _iData.GetData(_scanId, _regionId, channelInfo.ChannelId, _frameId.TimeId, scale, rect);
            Application.Current.Dispatcher.Invoke(() => Console.WriteLine("r-{0} t-{1} s-{2} c-{3}------------------", _regionId, _tileId, _frameId.StreamId, channelInfo.ChannelId));
            return data;
        }
        private ImageData ToComputeColor(ImageData data, Color color)
        {
            var n = data.Length;
            var computeData = new ImageData(data.XSize, data.YSize, false);
            for (int i = 0; i < n; i++)
            {
                computeData[i * 3] = (ushort)((double)color.B * (double)(data[i]) / 255.0);
                computeData[i * 3 + +1] = (ushort)((double)color.G * (double)(data[i]) / 255.0);
                computeData[i * 3 + 2] = (ushort)((double)color.R * (double)(data[i]) / 255.0);
            }
            return computeData;
        }
        private ImageData getVirtualData(VirtualChannel channel, Dictionary<Channel, ImageData> dic)
        {
            ImageData data1 = null, data2 = null;
            if (!channel.FirstChannel.IsvirtualChannel)
                data1 = dic[channel.FirstChannel];
            else
                data1 = getVirtualData(channel.FirstChannel as VirtualChannel, dic);

            if (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak)
            {
                if (!channel.SecondChannel.IsvirtualChannel)
                    data2 = dic[channel.SecondChannel];
                else
                    data2 = getVirtualData(channel.SecondChannel as VirtualChannel, dic);
            }
            if (data1 == null || (data2 == null && (channel.Operator != ImageOperator.Multiply && channel.Operator != ImageOperator.Invert && channel.Operator != ImageOperator.ShiftPeak))) return null;
            ImageData result = new ImageData(data1.XSize, data1.YSize);
            switch (channel.Operator)
            {
                case ImageOperator.Add:
                    result = data1.Add(data2, _maxBits);
                    break;
                case ImageOperator.Subtract:
                    result = data1.Sub(data2);
                    break;
                case ImageOperator.Invert:
                    result = data1.Invert(_maxBits);
                    break;
                case ImageOperator.Max:
                    result = data1.Max(data2);
                    break;
                case ImageOperator.Min:
                    result = data1.Min(data2);
                    break;
                case ImageOperator.Multiply:
                    result = data1.MulConstant(channel.Operand, _maxBits);
                    break;
                case ImageOperator.ShiftPeak:
                    result = data1.ShiftPeak((ushort)channel.Operand, _maxBits);
                    break;
                default:
                    break;
            }
            return result;
        }
        private ImageData getComputeData(Dictionary<Channel, Tuple<ImageData, Color>> dic, Int32Rect rect, double scale)
        {
            var data = new ImageData((uint)(rect.Width * scale), (uint)(rect.Height * scale), false);
            foreach (var o in dic)
            {
                var d = ToComputeColor(o.Value.Item1, o.Value.Item2);
                data = data.Add(d, _maxBits);
                d.Dispose();
            }
            return data;
        }
    }

}

