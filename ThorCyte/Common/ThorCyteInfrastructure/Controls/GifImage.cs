using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ThorCyte.Infrastructure.Controls
{
    public class GifImage : System.Windows.Controls.Image
    {
        public static readonly DependencyProperty FrameIndexProperty = DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));
        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }
        public static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var ob = obj as GifImage;
            ob.Source = ob._gf.Frames[(int)ev.NewValue];
            ob.InvalidateVisual();
        }
        private GifBitmapDecoder _gf;
        private Int32Animation _anim;
        private bool _animationIsWorking = false;
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Uri uri;
            if (Source == null)
            {
                uri = new Uri("pack://application:,,,/ThorCyte.Infrastructure;component/Resources/loading.gif", UriKind.Absolute);
            }
            else
            {
                uri = new Uri(Source.ToString());
            }
            _gf = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _anim = new Int32Animation(0, _gf.Frames.Count - 1,
                                       new Duration(new TimeSpan(0, 0, 0, _gf.Frames.Count / 5, (int)((_gf.Frames.Count / 5.0 - _gf.Frames.Count / 5) * 1000)))) { RepeatBehavior = RepeatBehavior.Forever };
            Source = _gf.Frames[0];
        }
        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            if (!_animationIsWorking)
            {
                BeginAnimation(FrameIndexProperty, _anim);
                _animationIsWorking = true;
            }
        }
    }
}
