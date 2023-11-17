using Microsoft.Xaml.Behaviors;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Illumine.LPR
{
    public class RowHeaderIndexBehavior : Behavior<DataGrid>
    {
        public Action RefreshItemAction
        {
            get { return (Action)GetValue(RefreshItemProperty); }
            set { SetValue(RefreshItemProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppendTextAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RefreshItemProperty =
            DependencyProperty.Register("RefreshItemAction", typeof(Action), typeof(RowHeaderIndexBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            SetCurrentValue(RefreshItemProperty, new Action(() =>
            {
                SetRowHeaderIndex(AssociatedObject);
            }));
            base.OnAttached();
        }

        private void SetRowHeaderIndex(DataGrid dataGrid)
        {
            for (int i = 0; i < dataGrid.Items.Count; i++)
            {
                var row = dataGrid.ItemContainerGenerator.ContainerFromIndex(i) as DataGridRow;
                if (row == null || row.DataContext == CollectionView.NewItemPlaceholder)
                    continue;
                row.Header = (i + 1).ToString();
            }
        }
    }
}
