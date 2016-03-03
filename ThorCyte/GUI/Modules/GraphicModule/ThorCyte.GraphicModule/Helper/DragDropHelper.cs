using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using ThorCyte.GraphicModule.Controls;
using ThorCyte.GraphicModule.Utils;

namespace ThorCyte.GraphicModule.Helper
{
    #region DragDropHelper

    /// <summary>
    /// Manages the dragging and dropping of ListBoxItems in a ListBox.
    /// The ItemType type parameter indicates the type of the objects in
    /// the ListBox's items source.  The ListBox's ItemsSource must be 
    /// set to an instance of ObservableCollection of ItemType, or an 
    /// Exception will be thrown.
    /// </summary>
    /// <typeparam name="TItemType">The type of the ListBox's items.</typeparam>
    public class DragDropHelper<TItemType> where TItemType : class
    {
        #region Fields

        private bool _canInitiateDrag;
        private DragAdorner _dragAdorner;
        private double _dragAdornerOpacity;
        private int _indexToSelect;
        private bool _isDragInProgress;
        private TItemType _itemUnderDragCursor;
        private ListBox _listBox;
        private Point _ptMouseDown;
        private bool _showDragAdorner;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of ListBoxDragManager.
        /// </summary>
        public DragDropHelper()
        {
            _canInitiateDrag = false;
            _dragAdornerOpacity = 0.5;
            _indexToSelect = -1;
            _showDragAdorner = true;
        }

        /// <summary>
        /// Initializes a new instance of ListBoxDragManager.
        /// </summary>
        /// <param name="listBox"></param>
        public DragDropHelper(ListBox listBox)
            : this()
        {
            ListBox = listBox;
        }

        /// <summary>
        /// Initializes a new instance of ListBoxDragManager.
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="dragAdornerOpacity"></param>
        public DragDropHelper(ListBox listBox, double dragAdornerOpacity)
            : this(listBox)
        {
            DragAdornerOpacity = dragAdornerOpacity;
        }

        /// <summary>
        /// Initializes a new instance of ListBoxDragManager.
        /// </summary>
        /// <param name="listBox"></param>
        /// <param name="showDragAdorner"></param>
        public DragDropHelper(ListBox listBox, bool showDragAdorner)
            : this(listBox)
        {
            ShowDragAdorner = showDragAdorner;
        }

        #endregion // Constructors

        #region properties

        /// <summary>
        /// Gets/sets the opacity of the drag adorner.  This property has no
        /// effect if ShowDragAdorner is false. The default value is 0.7
        /// </summary>
        public double DragAdornerOpacity
        {
            get { return _dragAdornerOpacity; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the DragAdornerOpacity property during a drag operation.");

                if (value < 0.0 || value > 1.0)
                    throw new ArgumentOutOfRangeException("DragAdornerOpacity", value, "Must be between 0 and 1.");

                _dragAdornerOpacity = value;
            }
        }

        /// <summary>
        /// Returns true if there is currently a drag operation being managed.
        /// </summary>
        public bool IsDragInProgress
        {
            get { return _isDragInProgress; }
            private set { _isDragInProgress = value; }
        }

        /// <summary>
        /// Gets/sets the listBox whose dragging is managed.  This property
        /// can be set to null, to prevent drag management from occuring.  If
        /// the listBox's AllowDrop property is false, it will be set to true.
        /// </summary>
        public ListBox ListBox
        {
            get { return _listBox; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the listBox property during a drag operation.");

                if (_listBox != null)
                {
                    _listBox.PreviewMouseLeftButtonDown -= PreviewMouseLeftButtonDown;
                    _listBox.PreviewMouseMove -= PreviewMouseMove;
                    _listBox.DragOver -= DragOver;
                    _listBox.DragLeave -= DragLeave;
                    _listBox.DragEnter -= DragEnter;
                    _listBox.Drop -= Drop;
                }

                _listBox = value;

                if (_listBox != null)
                {
                    if (!_listBox.AllowDrop)
                        _listBox.AllowDrop = true;

                    _listBox.PreviewMouseLeftButtonDown += PreviewMouseLeftButtonDown;
                    _listBox.PreviewMouseMove += PreviewMouseMove;
                    _listBox.DragOver += DragOver;
                    _listBox.DragLeave += DragLeave;
                    _listBox.DragEnter += DragEnter;
                    _listBox.Drop += Drop;
                }
            }
        }


        /// <summary>
        /// Gets/sets whether a visual representation of the ListBoxItem being dragged
        /// follows the mouse cursor during a drag operation.  The default value is true.
        /// </summary>
        public bool ShowDragAdorner
        {
            get { return _showDragAdorner; }
            set
            {
                if (IsDragInProgress)
                    throw new InvalidOperationException("Cannot set the ShowDragAdorner property during a drag operation.");

                _showDragAdorner = value;
            }
        }

        private bool CanStartDragOperation
        {
            get
            {
                if (Mouse.LeftButton != MouseButtonState.Pressed)
                    return false;

                if (!_canInitiateDrag)
                    return false;

                if (_indexToSelect == -1)
                    return false;

                if (!HasCursorLeftDragThreshold)
                    return false;

                return true;
            }
        }

        private bool HasCursorLeftDragThreshold
        {
            get
            {
                if (_indexToSelect < 0)
                    return false;

                ListBoxItem item = GetListBoxItem(_indexToSelect);
                Rect bounds = VisualTreeHelper.GetDescendantBounds(item);
                Point ptInItem = _listBox.TranslatePoint(_ptMouseDown, item);

                // In case the cursor is at the very top or bottom of the ListBoxItem
                // we want to make the vertical threshold very small so that dragging
                // over an adjacent item does not select it.
                double topOffset = Math.Abs(ptInItem.Y);
                double btmOffset = Math.Abs(bounds.Height - ptInItem.Y);
                double vertOffset = Math.Min(topOffset, btmOffset);

                double width = SystemParameters.MinimumHorizontalDragDistance * 2;
                double height = Math.Min(SystemParameters.MinimumVerticalDragDistance, vertOffset) * 2;
                var szThreshold = new Size(width, height);

                var rect = new Rect(_ptMouseDown, szThreshold);
                rect.Offset(szThreshold.Width / -2, szThreshold.Height / -2);
                var ptInListView = MouseUtilities.GetMousePosition(_listBox);
                return !rect.Contains(ptInListView);
            }
        }

        /// <summary>
        /// Returns the index of the ListBoxItem underneath the
        /// drag cursor, or -1 if the cursor is not over an item.
        /// </summary>
        private int IndexUnderDragCursor
        {
            get
            {
                int index = -1;
                for (int i = 0; i < _listBox.Items.Count; ++i)
                {
                    ListBoxItem item = GetListBoxItem(i);
                    if (IsMouseOver(item))
                    {
                        index = i;
                        break;
                    }
                }
                return index;
            }
        }

        /// <summary>
        /// Returns true if the mouse cursor is over a scrollbar in the listBox.
        /// </summary>
        private bool IsMouseOverScrollbar
        {
            get
            {
                Point ptMouse = MouseUtilities.GetMousePosition(_listBox);
                HitTestResult res = VisualTreeHelper.HitTest(_listBox, ptMouse);
                if (res == null)
                    return false;

                DependencyObject depObj = res.VisualHit;
                while (depObj != null)
                {
                    if (depObj is ScrollBar)
                        return true;

                    // VisualTreeHelper works with objects of type Visual or Visual3D.
                    // If the current object is not derived from Visual or Visual3D,
                    // then use the LogicalTreeHelper to find the parent element.
                    if (depObj is Visual || depObj is System.Windows.Media.Media3D.Visual3D)
                        depObj = VisualTreeHelper.GetParent(depObj);
                    else
                        depObj = LogicalTreeHelper.GetParent(depObj);
                }

                return false;
            }
        }

        private TItemType ItemUnderDragCursor
        {
            get { return _itemUnderDragCursor; }
            set
            {
                if (_itemUnderDragCursor == value)
                    return;

                // The first pass handles the previous item under the cursor.
                // The second pass handles the new one.
                for (int i = 0; i < 2; ++i)
                {
                    if (i == 1)
                        _itemUnderDragCursor = value;

                    if (_itemUnderDragCursor != null)
                    {
                        ListBoxItem listBoxItem = GetListBoxItem(_itemUnderDragCursor);
                        if (listBoxItem != null)
                            ListBoxItemDragState.SetIsUnderDragCursor(listBoxItem, i == 1);
                    }
                }
            }
        }

        private bool ShowDragAdornerResolved
        {
            get { return ShowDragAdorner && DragAdornerOpacity > 0.0; }
        }

        #endregion

        #region Event Handling Methods

        private void PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var graph = _listBox.SelectedItem as GraphicUcBase;
            if (graph == null)
            {
                return;
            }
            var canvas = graph.RegionPanel;
            if (canvas.Tool != ToolType.Pointer)
            {
                return;
            }

            if (IsMouseOverScrollbar)
            {
                // 4/13/2007 - Set the flag to false when cursor is over scrollbar.
                _canInitiateDrag = false;
                return;
            }

            int index = IndexUnderDragCursor;
            _canInitiateDrag = index > -1;

            if (_canInitiateDrag)
            {
                // Remember the location and index of the ListBoxItem the user clicked on for later.
                _ptMouseDown = MouseUtilities.GetMousePosition(_listBox);
                _indexToSelect = index;
            }
            else
            {
                _ptMouseDown = new Point(-10000, -10000);
                _indexToSelect = -1;
            }
        }

        private void PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var graph = _listBox.SelectedItem as GraphicUcBase;
            if (graph == null)
            {
                return;
            }
            var canvas = graph.RegionPanel;
            if (canvas.Selection.ToList().Count != 0 || canvas.Tool != ToolType.Pointer)
            {
                return;
            }
            if (_indexToSelect != IndexUnderDragCursor)
            {
                return;
            }
            if (!CanStartDragOperation)
                return;

            // Select the item the user clicked on.
            if (_listBox.SelectedIndex != _indexToSelect)
                _listBox.SelectedIndex = _indexToSelect;

            // If the item at the selected index is null, there's nothing
            // we can do, so just return;
            if (_listBox.SelectedItem == null)
                return;

            ListBoxItem itemToDrag = GetListBoxItem(_listBox.SelectedIndex);
            if (itemToDrag == null)
                return;

            AdornerLayer adornerLayer = ShowDragAdornerResolved ? InitializeAdornerLayer(itemToDrag) : null;
            InitializeDragOperation(itemToDrag);
            PerformDragOperation();
            FinishDragOperation(itemToDrag, adornerLayer);
        }

        private void DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;

            if (ShowDragAdornerResolved)
                UpdateDragAdornerLocation();

            // Update the item which is known to be currently under the drag cursor.
            int index = IndexUnderDragCursor;
            ItemUnderDragCursor = index < 0 ? null : ListBox.Items[index] as TItemType;
        }

        private void DragLeave(object sender, DragEventArgs e)
        {
            if (!IsMouseOver(_listBox))
            {
                if (ItemUnderDragCursor != null)
                    ItemUnderDragCursor = null;

                if (_dragAdorner != null)
                    _dragAdorner.Visibility = Visibility.Collapsed;
            }
        }

        private void DragEnter(object sender, DragEventArgs e)
        {
            if (_dragAdorner != null && _dragAdorner.Visibility != Visibility.Visible)
            {
                // Update the location of the adorner and then show it.				
                UpdateDragAdornerLocation();
                _dragAdorner.Visibility = Visibility.Visible;
            }
        }

        private void Drop(object sender, DragEventArgs e)
        {
            if (ItemUnderDragCursor != null)
                ItemUnderDragCursor = null;

            e.Effects = DragDropEffects.None;
            var type = _listBox.SelectedItem.GetType();

            if (!e.Data.GetDataPresent(type))
                return;

            // Get the data object which was dropped.
            TItemType data = e.Data.GetData(type) as TItemType;
            if (data == null)
                return;

            if (_listBox.Items == null)
                throw new Exception(
                    "A listBox managed by ListViewDragManager must have its ItemsSource set to an ObservableCollection<ItemType>.");

            int oldIndex = _listBox.Items.IndexOf(data);
            int newIndex = IndexUnderDragCursor;

            if (newIndex < 0)
            {
                // The drag started somewhere else, and our listBox is empty
                // so make the new item the first in the list.
                if (_listBox.Items.Count == 0)
                    newIndex = 0;

                // The drag started somewhere else, but our listBox has items
                // so make the new item the last in the list.
                else if (oldIndex < 0)
                    newIndex = _listBox.Items.Count;

                // The user is trying to drop an item from our listBox into
                // our listBox, but the mouse is not over an item, so don't
                // let them drop it.
                else
                    return;
            }

            // Dropping an item back onto itself is not considered an actual 'drop'.
            if (oldIndex == newIndex)
                return;


            // Move the dragged data object from it's original index to the
            // new index (according to where the mouse cursor is).  If it was
            // not previously in the ListBox, then insert the item.
            if (oldIndex > -1)
            {
                _listBox.Items.RemoveAt(oldIndex);
            }
            _listBox.Items.Insert(newIndex, data);
            // Set the Effects property so that the call to DoDragDrop will return 'Move'.
            e.Effects = DragDropEffects.Move;

        }

        #endregion

        #region Private Helpers

        private void FinishDragOperation(ListBoxItem draggedItem, AdornerLayer adornerLayer)
        {
            // Let the ListBoxItem know that it is not being dragged anymore.
            ListBoxItemDragState.SetIsBeingDragged(draggedItem, false);

            IsDragInProgress = false;

            if (ItemUnderDragCursor != null)
                ItemUnderDragCursor = null;

            // Remove the drag adorner from the adorner layer.
            if (adornerLayer != null)
            {
                adornerLayer.Remove(_dragAdorner);
                _dragAdorner = null;
            }
        }

        private ListBoxItem GetListBoxItem(int index)
        {
            if (_listBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listBox.ItemContainerGenerator.ContainerFromIndex(index) as ListBoxItem;
        }

        private ListBoxItem GetListBoxItem(TItemType dataItem)
        {
            if (_listBox.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                return null;

            return _listBox.ItemContainerGenerator.ContainerFromItem(dataItem) as ListBoxItem;
        }

        private AdornerLayer InitializeAdornerLayer(ListBoxItem itemToDrag)
        {
            // Create a brush which will paint the ListBoxItem onto
            // a visual in the adorner layer.
            var brush = new VisualBrush(itemToDrag);

            // Create an element which displays the source item while it is dragged.
            _dragAdorner = new DragAdorner(_listBox, itemToDrag.RenderSize, brush)
            {
                Opacity = DragAdornerOpacity  // Set the drag adorner's opacity.
            };

            AdornerLayer layer = AdornerLayer.GetAdornerLayer(_listBox);
            layer.Add(_dragAdorner);

            // Save the location of the cursor when the left mouse button was pressed.
            _ptMouseDown = MouseUtilities.GetMousePosition(_listBox);

            return layer;
        }

        private void InitializeDragOperation(ListBoxItem itemToDrag)
        {
            // Set some flags used during the drag operation.
            IsDragInProgress = true;
            _canInitiateDrag = false;

            // Let the ListBoxItem know that it is being dragged.
            ListBoxItemDragState.SetIsBeingDragged(itemToDrag, true);
        }

        private bool IsMouseOver(Visual target)
        {
            // We need to use MouseUtilities to figure out the cursor
            // coordinates because, during a drag-drop operation, the WPF
            // mechanisms for getting the coordinates behave strangely.

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);
            Point mousePos = MouseUtilities.GetMousePosition(target);
            return bounds.Contains(mousePos);
        }

        private void PerformDragOperation()
        {
            var selectedItem = _listBox.SelectedItem as TItemType;
            DragDropEffects allowedEffects = DragDropEffects.Move | DragDropEffects.Move | DragDropEffects.Link;

            if (DragDrop.DoDragDrop(_listBox, selectedItem, allowedEffects) != DragDropEffects.None)
            {
                // The item was dropped into a new location,
                // so make it the new selected item.
                _listBox.SelectedItem = selectedItem;
            }
        }

        private void UpdateDragAdornerLocation()
        {
            if (_dragAdorner != null)
            {
                Point ptCursor = MouseUtilities.GetMousePosition(ListBox);

                // 4/13/2007 - Made the top offset relative to the item being dragged.
                ListBoxItem itemBeingDragged = GetListBoxItem(_indexToSelect);
                Point itemLoc = itemBeingDragged.TranslatePoint(new Point(0, 0), ListBox);

                double left = itemLoc.X + ptCursor.X - _ptMouseDown.X;
                double top = itemLoc.Y + ptCursor.Y - _ptMouseDown.Y;
                _dragAdorner.SetOffsets(left, top);
            }
        }

        #endregion
    }

    #endregion

    #region ListBoxItemDragState

    /// <summary>
    /// Exposes attached properties used in conjunction with the ListViewDragDropManager class.
    /// Those properties can be used to allow triggers to modify the appearance of ListBoxItems
    /// in a listBox during a drag-drop operation.
    /// </summary>
    public static class ListBoxItemDragState
    {
        #region Dependency properties
        /// <summary>
        /// Identifies the ListBoxItemDragState's IsBeingDragged attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsBeingDraggedProperty =
            DependencyProperty.RegisterAttached("IsBeingDragged", typeof(bool), typeof(ListBoxItemDragState), new UIPropertyMetadata(false));


        /// <summary>
        /// Identifies the ListBoxItemDragState's IsUnderDragCursor attached property.  
        /// This field is read-only.
        /// </summary>
        public static readonly DependencyProperty IsUnderDragCursorProperty =
            DependencyProperty.RegisterAttached("IsUnderDragCursor", typeof(bool), typeof(ListBoxItemDragState), new UIPropertyMetadata(false));

        #endregion

        #region Methods

        /// <summary>
        /// Returns true if the specified ListBoxItem is being dragged, else false.
        /// </summary>
        /// <param name="item">The ListBoxItem to check.</param>
        public static bool GetIsBeingDragged(ListBoxItem item)
        {
            return (bool)item.GetValue(IsBeingDraggedProperty);
        }

        /// <summary>
        /// Sets the IsBeingDragged attached property for the specified ListBoxItem.
        /// </summary>
        /// <param name="item">The ListBoxItem to set the property on.</param>
        /// <param name="value">Pass true if the element is being dragged, else false.</param>
        internal static void SetIsBeingDragged(ListBoxItem item, bool value)
        {
            item.SetValue(IsBeingDraggedProperty, value);
        }

        /// <summary>
        /// Returns true if the specified ListBoxItem is currently underneath the cursor 
        /// during a drag-drop operation, else false.
        /// </summary>
        /// <param name="item">The ListBoxItem to check.</param>
        public static bool GetIsUnderDragCursor(ListBoxItem item)
        {
            return (bool)item.GetValue(IsUnderDragCursorProperty);
        }

        /// <summary>
        /// Sets the IsUnderDragCursor attached property for the specified ListViewItem.
        /// </summary>
        /// <param name="item">The ListBoxItem to set the property on.</param>
        /// <param name="value">Pass true if the element is underneath the drag cursor, else false.</param>
        internal static void SetIsUnderDragCursor(ListBoxItem item, bool value)
        {
            item.SetValue(IsUnderDragCursorProperty, value);
        }

        #endregion
    }

    #endregion
}