using System;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Controls;
using Abt.Controls.SciChart;
using Microsoft.Practices.Unity;
using ThorCyte.Statistic.Models;
using ThorCyte.Statistic.ViewModels;
using Abt.Controls.SciChart.ChartModifiers;
using System.Windows;
using System.Windows.Input;

namespace ThorCyte.Statistic.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class StatisticView : UserControl
    {
        public StatisticView(StatisticViewModel pVM, IUnityContainer container)
        {
            InitializeComponent();
            DataContext = pVM;
        }

    }
    public interface IPopupSetupWindow
    {
        bool PopupWindow();
        bool Close();
    }

    public class PopupSetupWindow : IPopupSetupWindow
    {
        private StatisticSetup _subwin;
        public bool PopupWindow()
        {
            _subwin = new StatisticSetup();
            return _subwin.ShowDialog() ?? false;
        }

        public bool Close()
        {
            if (_subwin != null)
            {
                _subwin.DialogResult = true;
                _subwin.Close();
                _subwin = null;
            }
            return true;
        }
    }

    public static class ZoomArrangeAid
    {
        public static readonly DependencyProperty IsZoomProperty = DependencyProperty.RegisterAttached(
            "IsZoom", typeof(bool), typeof(ZoomArrangeAid),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnZoomChange)));

        public static readonly DependencyProperty IsGExtendProperty = DependencyProperty.RegisterAttached(
            "IsGExtend", typeof(bool), typeof(ZoomArrangeAid),
            new FrameworkPropertyMetadata(new PropertyChangedCallback(OnNavigation)));


        public static bool IsCurrentExtendStatus = true;
        public static void SetIsZoom(DependencyObject obj, bool source)
        {
            obj.SetValue(IsZoomProperty, source);
        }

        public static bool GetIsZoom(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsZoomProperty);
        }
        public static void SetIsGExtend(DependencyObject obj, bool source)
        {
            obj.SetValue(IsGExtendProperty, source);
        }

        public static bool GetIsGExtend(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsGExtendProperty);
        }

        private static void OnZoomChange(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            UIElement sourceElement = depObj as UIElement;
            if (GetIsZoom(depObj))
            {
                sourceElement.PreviewMouseLeftButtonDown += ZommAid_PreviewMouseLeftButtonDown;
            }
            else
            {
                sourceElement.PreviewMouseLeftButtonDown -= ZommAid_PreviewMouseLeftButtonDown;
            }
        }

        private static void OnNavigation(DependencyObject depObj, DependencyPropertyChangedEventArgs args)
        {
            NavigationGrid = depObj as Grid;
        }

        private static Grid NavigationGrid = null;
        private static void ZommAid_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                IsCurrentExtendStatus = !IsCurrentExtendStatus;
                (sender as Panel).InvalidateMeasure();
                if (NavigationGrid != null)
                {
                    if (IsCurrentExtendStatus)
                    {
                        NavigationGrid.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        NavigationGrid.Visibility = Visibility.Visible;
                    }
                }
            }
        }
    }

    public class ZStackPanel : Panel
    {
        public static readonly DependencyProperty ZFactorProperty = DependencyProperty.Register(
            "ZFactor", typeof(double), typeof(ZStackPanel),
            new FrameworkPropertyMetadata(0.8D, FrameworkPropertyMetadataOptions.AffectsArrange));

        public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register(
            "ItemHeight", typeof(double), typeof(ZStackPanel),
            new FrameworkPropertyMetadata(30.0D, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                    FrameworkPropertyMetadataOptions.AffectsArrange));
        public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register(
            "ItemWidth", typeof(double), typeof(ZStackPanel),
            new FrameworkPropertyMetadata(30.0D, FrameworkPropertyMetadataOptions.AffectsMeasure |
                                                    FrameworkPropertyMetadataOptions.AffectsArrange));


        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        public double ZFactor
        {
            get { return (double)GetValue(ZFactorProperty); }
            set { SetValue(ZFactorProperty, value); }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            if (double.IsPositiveInfinity(constraint.Width) || double.IsPositiveInfinity(constraint.Height))
                return Size.Empty;

            //foreach (UIElement child in InternalChildren)
            //{
            //    child.d
            //    Size childSize = new Size(constraint.Width, ItemHeight);
            //    child.Measure(childSize);
            //}

            return new Size(constraint.Width, constraint.Height);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            int currentIndex = 0;

            for (int index = InternalChildren.Count - 1; index >= 0; index--)
            {
                Rect rect = CalculateRect(finalSize, currentIndex);
                InternalChildren[index].Arrange(rect);

                currentIndex++;
            }
            return finalSize;
        }

        private Rect CalculateRect(Size panelSize, int index)
        {
            double zFactor = 1;
            double tHeight = 0;
            if (ZoomArrangeAid.IsCurrentExtendStatus)
            {
                index = 0;
                tHeight = panelSize.Height;
            }
            else
            {
                zFactor = Math.Pow(ZFactor, index + 1);
                tHeight = panelSize.Height * zFactor;
            }

            Size itemSize = new Size(panelSize.Width * zFactor, tHeight);

            double left = (panelSize.Width - itemSize.Width + (index == 0 ? 0 : (panelSize.Width * ZFactor - itemSize.Width)) + 20 * index) * 0.5;
            double top = (panelSize.Height - itemSize.Height) * 0.5;

            Rect rect = new Rect(itemSize);
            rect.Location = new Point(left, top);
            return rect;
        }
    }
}
