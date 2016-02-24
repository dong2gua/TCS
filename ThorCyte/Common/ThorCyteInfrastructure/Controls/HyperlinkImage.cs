using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ThorCyte.Infrastructure.Exceptions;

namespace ThorCyte.Infrastructure.Controls
{
    public class HyperlinkImage : Image
    {
        public static readonly DependencyProperty NavigateUriProperty =
                               DependencyProperty.Register("NavigateUri", typeof(string), typeof(HyperlinkImage), new PropertyMetadata());

        public string NavigateUri
        {
            get { return (string)GetValue(NavigateUriProperty); }
            set { SetValue(NavigateUriProperty, value); }
        }

        public HyperlinkImage()
        {
            this.Cursor = Cursors.Hand;
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            base.OnMouseEnter(e);
            var url = this.NavigateUri;
            ThreadPool.QueueUserWorkItem(w =>
            {
                try
                {
                    var pro = new Process
                    {
                        StartInfo =
                        {
                            WindowStyle = ProcessWindowStyle.Normal,
                            UseShellExecute = true,
                            FileName = url,
                            ErrorDialog = true
                        }
                    };
                    pro.Start();
                }
                catch (Exception ex)
                {
                    CyteException exception = new CyteException("HyperlinkImage","error");
                }
            });
        }

    }
}
