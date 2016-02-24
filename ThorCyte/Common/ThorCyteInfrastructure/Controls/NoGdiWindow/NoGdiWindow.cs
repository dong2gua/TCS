namespace System.Windows.Controls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using Mvvm;

    /// <summary>
    /// Removes all GDI stuff from a WPF Window, and makes it resizeable.
    /// </summary>
    /// <remarks>Root visual should be a Grid.</remarks>
    public partial class NoGdiWindow : Window, INotifyPropertyChanged 
    {
        /// <summary>
        /// Win32 Handle.
        /// </summary>
        private HwndSource hwndSource;

        /// <summary>
        /// Close Command
        /// </summary>
        private DelegateCommand closeCommand;

        /// <summary>
        /// Maximize Command
        /// </summary>
        private DelegateCommand maximizeCommand;

        /// <summary>
        /// Minimize Command
        /// </summary>
        private DelegateCommand minimizeCommand;

        private DelegateCommand moveHeaderCommand;
        /// <summary>
        /// Links a cursor to each direction.
        /// </summary>
        private Dictionary<ResizeDirection, Cursor> cursors = new Dictionary<ResizeDirection, Cursor> 
        {
            { ResizeDirection.Top, Cursors.SizeNS },
            { ResizeDirection.Bottom, Cursors.SizeNS },
            { ResizeDirection.Left, Cursors.SizeWE },
            { ResizeDirection.Right, Cursors.SizeWE },
            { ResizeDirection.TopLeft, Cursors.SizeNWSE },
            { ResizeDirection.BottomRight, Cursors.SizeNWSE },
            { ResizeDirection.TopRight, Cursors.SizeNESW },
            { ResizeDirection.BottomLeft, Cursors.SizeNESW } 
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="NoGdiWindow"/> class.
        /// </summary>
        public NoGdiWindow()
        {
            // Commands
            this.closeCommand = new Mvvm.DelegateCommand(this.Close_Executed);
            this.maximizeCommand = new Mvvm.DelegateCommand(this.Maximize_Executed);
            this.minimizeCommand = new Mvvm.DelegateCommand(this.Minimize_Executed);
            this.moveHeaderCommand = new DelegateCommand(this.MoveHeader_Excuted);

            // Events
            this.SourceInitialized += this.ResizeableWindow_SourceInitialized;
            this.Initialized += this.ResizeableWindow_Initialized;
            this.StateChanged += this.ResizeableWindow_StateChanged;
        }

        private void MoveHeader_Excuted(object obj)
        {
            MouseButtonEventArgs e = obj as MouseButtonEventArgs;
            if (e.ClickCount > 1 && this.ResizeMode != ResizeMode.NoResize)
            {
                this.MaximizeCommand.Execute(null);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }

        }

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Resize Direction
        /// </summary>
        private enum ResizeDirection
        {
            Left = 1,
            Right = 2,
            Top = 3,
            TopLeft = 4,
            TopRight = 5,
            Bottom = 6,
            BottomLeft = 7,
            BottomRight = 8,
        }

        public bool UsesWindowsFormsHost { get; set; }

        /// <summary>
        /// Gets the close command.
        /// </summary>
        public ICommand CloseCommand
        {
            get { return this.closeCommand; }
        }

        /// <summary>
        /// Gets the maximize command.
        /// </summary>
        public ICommand MaximizeCommand
        {
            get { return this.maximizeCommand; }
        }

        /// <summary>
        /// Gets the minimize command.
        /// </summary>
        public ICommand MinimizeCommand
        {
            get { return this.minimizeCommand; }
        }


        public ICommand MoveHeaderCommand
        {
            get { return this.moveHeaderCommand; }
        }
        /// <summary>
        /// Gets the visibility of the maximize button.
        /// </summary>
        public Visibility MaximizeButtonVisibility
        {
            get
            {
                if (this.WindowState == System.Windows.WindowState.Normal && this.ResizeMode!=ResizeMode.NoResize)
                {
                    return System.Windows.Visibility.Visible;
                }

                return System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Gets the visibility of the restore button.
        /// </summary>
        public Visibility RestoreButtonVisibility
        {
            get
            {
                if (this.WindowState == System.Windows.WindowState.Maximized && this.ResizeMode != ResizeMode.NoResize)
                {
                    return System.Windows.Visibility.Visible;
                }

                return System.Windows.Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Handles the MouseLeftButtonDown event of the header part of the window.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseButtonEventArgs"/> instance containing the event data.</param>
        protected void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1 && this.ResizeMode != ResizeMode.NoResize)
            {
                this.MaximizeCommand.Execute(null);
            }
            else if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        /// <summary>
        /// Gets the resize direction from the name of the handle rectangle.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>A ResizeDirection.</returns>
        private static ResizeDirection GetDirectionFromName(string name)
        {
            // Assumes the drag handels are all named *DragHandle
            string enumName = name.Replace("DragHandle", string.Empty);
            return (ResizeDirection)Enum.Parse(typeof(ResizeDirection), enumName);
        }

        /// <summary>
        /// Handles the Initialized event of the ResizeableWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        protected virtual void ResizeableWindow_Initialized(object sender, EventArgs e)
        {
            //// Visual Properties
            //this.WindowStyle = System.Windows.WindowStyle.None;

            //// Real transparency only works above Windows XP and if there are no WindowsFormsHost controls
            //if ((Environment.OSVersion.Version.Major > 5) & (!this.UsesWindowsFormsHost))
            //{
            //    Grid root = this.Content as Grid;

            //    if (root != null)
            //    {
            //        this.AllowsTransparency = true;
            //        this.Background = new SolidColorBrush(Colors.Transparent);

            //        root.Children.Add(this.GetResizeHandleRectangle("TopDragHandle", HorizontalAlignment.Stretch, VerticalAlignment.Top));
            //        root.Children.Add(this.GetResizeHandleRectangle("RightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Stretch));
            //        root.Children.Add(this.GetResizeHandleRectangle("BottomDragHandle", HorizontalAlignment.Stretch, VerticalAlignment.Bottom));
            //        root.Children.Add(this.GetResizeHandleRectangle("LeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Stretch));
            //        root.Children.Add(this.GetResizeHandleRectangle("TopLeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Top));
            //        root.Children.Add(this.GetResizeHandleRectangle("TopRightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Top));
            //        root.Children.Add(this.GetResizeHandleRectangle("BottomRightDragHandle", HorizontalAlignment.Right, VerticalAlignment.Bottom));
            //        root.Children.Add(this.GetResizeHandleRectangle("BottomLeftDragHandle", HorizontalAlignment.Left, VerticalAlignment.Bottom));
            //    }
            //}
        }

        /// <summary>
        /// Handles the SourceInitialized event of the ResizeableWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ResizeableWindow_SourceInitialized(object sender, EventArgs e)
        {
            this.hwndSource = PresentationSource.FromVisual((Visual)sender) as HwndSource;
        }

        /// <summary>
        /// Resizes the ResizeableWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Input.MouseEventArgs"/> instance containing the event data.</param>
        private void ResizeableWindow_ResizeIfPressed(object sender, MouseEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            ResizeDirection direction = GetDirectionFromName(element.Name);

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                SendMessage(this.hwndSource.Handle, 0x112, (IntPtr)(61440 + direction), IntPtr.Zero);
            }
        }

        /// <summary>
        /// Handles the StateChanged event of the ResizeableWindow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void ResizeableWindow_StateChanged(object sender, EventArgs e)
        {
            this.PropertyChanged.RaiseAll(this);
        }

        /// <summary>
        /// Close executed.
        /// </summary>
        /// <param name="o">The ignored parameter.</param>
        private void Close_Executed(object o)
        {
            this.Close();
        }

        /// <summary>
        /// Maximize executed.
        /// </summary>
        /// <param name="o">The o.</param>
        private void Maximize_Executed(object o)
        {
            MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
            if (this.WindowState == System.Windows.WindowState.Maximized)
            {
                this.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                this.WindowState = System.Windows.WindowState.Maximized;
            }
        }

        /// <summary>
        /// Minimize executed.
        /// </summary>
        /// <param name="o">The o.</param>
        private void Minimize_Executed(object o)
        {
            this.WindowState = System.Windows.WindowState.Minimized;
        }

        /// <summary>
        /// Gets a resizehandle rectangle.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="hAlign">The horizontal alignment.</param>
        /// <param name="vAlign">The vertical alignment.</param>
        /// <returns>A resizehandle rectangle.</returns>
        protected Rectangle GetResizeHandleRectangle(string name, HorizontalAlignment hAlign, VerticalAlignment vAlign)
        {
            Rectangle rect = new Rectangle();
            rect.Fill = new SolidColorBrush(Colors.Transparent);
            rect.Stroke = new SolidColorBrush(Colors.Transparent);
            rect.MinHeight = 4;
            rect.MinWidth = 4;
            rect.MouseMove += this.ResizeableWindow_ResizeIfPressed;
            rect.PreviewMouseDown += this.ResizeableWindow_ResizeIfPressed;
            rect.Name = name;
            rect.HorizontalAlignment = hAlign;
            rect.VerticalAlignment = vAlign;
            ResizeDirection direction = GetDirectionFromName(name);
            rect.Cursor = this.cursors[direction];
            return rect;
        }
    }
}