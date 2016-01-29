using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace ThorCyte.ImageViewerModule.Control
{
    public class SplitButton : Button
    {
        public static readonly DependencyProperty DropDownContentProperty = DependencyProperty.Register("DropDownContent", typeof(object), typeof(SplitButton), new PropertyMetadata(null, OnDropDownContentChanged));
        public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register("IsOpen", typeof(bool), typeof(SplitButton), new PropertyMetadata(false, OnIsOpenChanged));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(SplitButton), new PropertyMetadata(Orientation.Horizontal));
        public object DropDownContent
        {
            get { return (object)GetValue(DropDownContentProperty); }
            set { SetValue(DropDownContentProperty, value); }
        }
        public bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }
        private Popup dropDownPopup;
        private ButtonBase dropDownButton;
        public SplitButton()
        {
            dropDownPopup = new Popup();
            dropDownPopup.PlacementTarget = this;
            dropDownPopup.StaysOpen = false;
            dropDownPopup.Closed += DropDownPopup_Closed;
        }
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            dropDownButton = this.Template.FindName("PART_DropDown", this) as ButtonBase;
            if (dropDownButton == null) return;
            dropDownButton.Click += Dropdown_Click;
            dropDownPopup.PlacementTarget = dropDownButton;
        }
        private static void OnDropDownContentChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var splitButton = property as SplitButton;
            var element = args.NewValue as UIElement;
            if (element == null) return;
            splitButton.dropDownPopup.Child = element;
        }
        private static void OnIsOpenChanged(DependencyObject property, DependencyPropertyChangedEventArgs args)
        {
            var splitButton = property as SplitButton;
            var isOpen = args.NewValue as bool?;
            splitButton.dropDownPopup.IsOpen = isOpen == true;

        }
        private void DropDownPopup_Closed(object sender, EventArgs e)
        {
            IsOpen = false;
        }
        private void Dropdown_Click(object sender, RoutedEventArgs e)
        {
            IsOpen = true;
            dropDownPopup.Focus();
            e.Handled = true;
        }
    }
}


