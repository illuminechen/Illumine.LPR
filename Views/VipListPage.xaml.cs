using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Illumine.LPR
{
    public partial class VipListPage : UserControl
    {
        public VipListPage()
        {
            InitializeComponent();
        }

        private void dataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshIndex();
        }

        private void dataGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            RefreshIndex();
        }

        private void RefreshIndex()
        {
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (row == null || row.DataContext == CollectionView.NewItemPlaceholder)
                    continue;
                row.Header = (i + 1).ToString();
            }
        }

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ((VipListPageViewModel)(this.DataContext)).CloseGroup();
        }

        private void dataGrid2_MouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        //private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    DataGridCell cell = sender as DataGridCell;
        //    if (cell == null || cell.IsEditing || cell.IsReadOnly)
        //        return;

        //    if (!cell.IsFocused)
        //    {
        //        cell.Focus();
        //    }

        //    if (cell.Content is ComboBox cb)
        //    {
        //        if (cb != null)
        //        {
        //            //DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
        //            dataGrid.BeginEdit(e);
        //            cell.Dispatcher.Invoke(
        //             DispatcherPriority.Background,
        //             new Action(delegate { }));
        //            cb.IsDropDownOpen = true;
        //        }
        //    }

        //}

        private void DataGridCell_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            GridColumnFastEdit(cell, e);
        }

        private void DataGridCell_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DataGridCell cell = sender as DataGridCell;
            GridColumnFastEdit(cell, e);
        }

        private static void GridColumnFastEdit(DataGridCell cell, RoutedEventArgs e)
        {
            if (cell == null || cell.IsEditing || cell.IsReadOnly)
                return;

            DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
            if (dataGrid == null)
                return;

            if (!cell.IsFocused)
            {
                cell.Focus();
            }

            if (cell.Content is CheckBox)
            {
                if (dataGrid.SelectionUnit != DataGridSelectionUnit.FullRow)
                {
                    if (!cell.IsSelected)
                        cell.IsSelected = true;
                }
                else
                {
                    DataGridRow row = FindVisualParent<DataGridRow>(cell);
                    if (row != null && !row.IsSelected)
                    {
                        row.IsSelected = true;
                    }
                }
            }
            else
            {
                ComboBox cb = cell.Content as ComboBox;
                if (cb != null)
                {
                    //DataGrid dataGrid = FindVisualParent<DataGrid>(cell);
                    dataGrid.BeginEdit(e);
                    cell.Dispatcher.Invoke(
                     DispatcherPriority.Background,
                     new Action(delegate { }));
                    cb.IsDropDownOpen = true;
                }
            }
        }


        private static T FindVisualParent<T>(UIElement element) where T : UIElement
        {
            UIElement parent = element;
            while (parent != null)
            {
                T correctlyTyped = parent as T;
                if (correctlyTyped != null)
                {
                    return correctlyTyped;
                }

                parent = VisualTreeHelper.GetParent(parent) as UIElement;
            }
            return null;
        }
    }
}
