﻿using System.Windows;
using System.Windows.Media;

namespace ThorCyte.ImageViewerModule.View
{
    /// <summary>
    /// Interaction logic for BrightnessContrastWindow.xaml
    /// </summary>
    public partial class BrightnessContrastWindow : Window
    {
        public BrightnessContrastWindow()
        {
            InitializeComponent();
            ColorScale.Fill= new LinearGradientBrush(Colors.Black, Colors.White, 0);
        }
    }
}
