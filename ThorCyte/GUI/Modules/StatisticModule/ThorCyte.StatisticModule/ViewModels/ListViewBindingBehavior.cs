using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ThorCyte.Statistic.ViewModels
{
    public class DataGridColumns
    {
        public string DisplayColumnName { get; set; }
        public string BindingPropertyName { get; set; }
        public int Width { get; set; }
    }

    public class ListViewBindingBehavior
    {
        //Build Attached DependencyProperty ColumnsCollection  
        public static readonly DependencyProperty ColumnsCollectionProperty =
            DependencyProperty.RegisterAttached("ColumnsCollection", typeof(ObservableCollection<DataGridColumns>), typeof(ListViewBindingBehavior), new PropertyMetadata(OnColumnsCollectionChanged));

        public static void SetColumnsCollection(DependencyObject o, ObservableCollection<ColumnDefinition> value)
        {
            o.SetValue(ColumnsCollectionProperty, value);
        }

        public static ObservableCollection<ColumnDefinition> GetColumnsCollection(DependencyObject o)
        {
            return o.GetValue(ColumnsCollectionProperty) as ObservableCollection<ColumnDefinition>;
        }

        private static void OnColumnsCollectionChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            ListView gridView = o as ListView;

            if (gridView == null)
            {
                return;
            }

            var listviewGridview = new GridView();
            gridView.View = listviewGridview;
            listviewGridview.Columns.Clear();

            if (gridView.ItemsSource == null)
            {
                return;
            }

            ObservableCollection<ExpandoObject> objExpando = (ObservableCollection<ExpandoObject>)gridView.ItemsSource;

            if (e.NewValue == null)
            {
                return;
            }

            var collection = e.NewValue as ObservableCollection<DataGridColumns>;

            if (collection == null)
            {
                return;
            }
            foreach (var column in collection)
            {
                var gridViewColumn = GetDataColumn(column);
                listviewGridview.Columns.Add(gridViewColumn);
            }
        }
        private static GridViewColumn GetDataColumn(DataGridColumns columnName)
        {
            var column = new GridViewColumn();
            column.Width = columnName.Width;
            column.Header = columnName.DisplayColumnName;
            var bd = new Binding();
            column.DisplayMemberBinding = bd;
            ((Binding)column.DisplayMemberBinding).Converter = new ColumnValueConverter();
            ((Binding)column.DisplayMemberBinding).ConverterParameter = columnName.BindingPropertyName;
            return column;
        }
    }

    public class ColumnValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var row = value as IDictionary<string, object>;
            if (row == null)
                return null;
            string columnName = (string)parameter;
            return (row[columnName]);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }     
}
