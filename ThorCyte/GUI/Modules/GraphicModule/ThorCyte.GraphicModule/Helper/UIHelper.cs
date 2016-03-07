using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using MessageBox = Xceed.Wpf.Toolkit.MessageBox;

namespace ThorCyte.GraphicModule.Helper
{
    public static class UIHelper
    {
        public static void OnCheckTabNameFailed(string name)
        {
            var action = new Action(() => MessageBox.Show(string.Format("The tab name {0} has exist.", name), "Message"));

            if (Dispatcher.CurrentDispatcher.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.CurrentDispatcher.BeginInvoke(action);
            }
        }

        /// <summary>  
        /// 可以对WPF中的控件抓取为图片形式.  
        /// </summary>  
        /// <param name="element">控件对象</param>  
        /// <param name="fileName">生成图片的路径</param>  
        public static void SaveToImage(FrameworkElement element, string fileName)
        {
            using (var fs = new FileStream(fileName, FileMode.Create))
            {
                var bmp = new RenderTargetBitmap((int)element.ActualWidth, (int)element.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);
                bmp.Render(element);
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bmp));
                encoder.Save(fs);
                fs.Close();
                fs.Dispose();
            }
        }

        public static void CopyUiElementToClipboard(FrameworkElement ui)
        {
            double width = ui.ActualWidth;
            double height = ui.ActualHeight;
            var bmp = new RenderTargetBitmap((int)Math.Round(width),
                (int)Math.Round(height), 96, 96, PixelFormats.Default);
            var dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                var vb = new VisualBrush(ui);
                dc.DrawRectangle(vb, null,
                      new Rect(new Point(), new Size(width, height)));
            }
            bmp.Render(dv);
            Clipboard.SetImage(bmp);
        }
    }
}