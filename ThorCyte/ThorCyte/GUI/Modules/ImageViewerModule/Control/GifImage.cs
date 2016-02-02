using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ThorCyte.ImageViewerModule.Control
{
    public class GifImage : System.Windows.Controls.Image
    {
        public int FrameIndex
        {
            get { return (int)GetValue(FrameIndexProperty); }
            set { SetValue(FrameIndexProperty, value); }
        }

        public static readonly DependencyProperty FrameIndexProperty =
            DependencyProperty.Register("FrameIndex", typeof(int), typeof(GifImage), new UIPropertyMetadata(0, new PropertyChangedCallback(ChangingFrameIndex)));

        static void ChangingFrameIndex(DependencyObject obj, DependencyPropertyChangedEventArgs ev)
        {
            var ob = obj as GifImage;
            ob.Source = ob._gf.Frames[(int)ev.NewValue];
            ob.InvalidateVisual();
        }
        GifBitmapDecoder _gf;
        Int32Animation _anim;
        public GifImage()
        {

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            _gf = new GifBitmapDecoder(new Uri(this.Source.ToString()), BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.Default);
            _anim = new Int32Animation(0, _gf.Frames.Count - 1,
                                       new Duration(new TimeSpan(0, 0, 0, _gf.Frames.Count / 5,
                                                                 (int)
                                                                 ((_gf.Frames.Count / 5.0 - _gf.Frames.Count / 5) * 1000))))
            { RepeatBehavior = RepeatBehavior.Forever };
            Source = _gf.Frames[0];
        }

        bool _animationIsWorking = false;
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
