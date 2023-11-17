using Microsoft.Xaml.Behaviors;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Illumine.LPR
{
    public class SearchBehavior : Behavior<DataGrid>
    {
        public Action<string> SearchNextAction
        {
            get { return (Action<string>)GetValue(SearchNextProperty); }
            set { SetValue(SearchNextProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppendTextAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchNextProperty =
            DependencyProperty.Register("SearchNextAction", typeof(Action<string>), typeof(SearchBehavior), new PropertyMetadata(null));
        public Action<string> SearchPreAction
        {
            get { return (Action<string>)GetValue(SearchPreProperty); }
            set { SetValue(SearchPreProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppendTextAction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchPreProperty =
            DependencyProperty.Register("SearchPreAction", typeof(Action<string>), typeof(SearchBehavior), new PropertyMetadata(null));

        protected override void OnAttached()
        {
            SetCurrentValue(SearchNextProperty, new Action<string>((s) =>
            {
                FocusNextRow(AssociatedObject, s);
            }));

            SetCurrentValue(SearchPreProperty, new Action<string>((s) =>
            {
                FocusPreRow(AssociatedObject, s);
            }));
            base.OnAttached();
        }
        private void FocusPreRow(DataGrid dataGrid, string str)
        {
            var cur = dataGrid.CurrentItem;
            var rows = dataGrid.Items;
            bool found = false || (cur == null);
            bool moved = false;
            for (int i = rows.Count - 1; i >= 0; i--)
            {
                var row = rows[i];
                if (found)
                {
                    if(row is VipViewModel vip)
                    {
                        if(vip.PlateNumber.Contains(str) || vip.Name.Contains(str))
                        {
                            dataGrid.CurrentItem = row;
                            dataGrid.SelectedItem = row;
                            dataGrid.ScrollIntoView(row);
                            moved = true;
                            break;
                        }
                    }    
                }
                if (dataGrid.CurrentItem == row)
                    found = true;
            }

            if (!moved && rows.Count > 0)
            {
                for (int i = rows.Count - 1; i >= 0; i--)
                {
                    var row = rows[i];
                    if (row is VipViewModel vip)
                    {
                        if (vip.PlateNumber.Contains(str) || vip.Name.Contains(str))
                        {
                            dataGrid.CurrentItem = row;
                            dataGrid.SelectedItem = row;
                            dataGrid.ScrollIntoView(row);
                            break;
                        }
                    }
                }
            }
        }
        private void FocusNextRow(DataGrid dataGrid, string str)
        {
            var cur = dataGrid.CurrentItem;
            var rows = dataGrid.Items;
            bool found = false || (cur == null);
            bool moved = false;
            for (int i = 0; i < rows.Count;i++)
            {
                var row = rows[i];
                if (found)
                {
                    if (row is VipViewModel vip)
                    {
                        if (vip.PlateNumber.Contains(str) || vip.Name.Contains(str))
                        {
                            dataGrid.CurrentItem = row;
                            dataGrid.SelectedItem = row;
                            dataGrid.ScrollIntoView(row);
                            moved = true;
                            break;
                        }
                    }
                }
                if (cur == row)
                    found = true;
            }

            if (!moved && rows.Count > 0)
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    if (row is VipViewModel vip)
                    {
                        if (vip.PlateNumber.Contains(str) || vip.Name.Contains(str))
                        {
                            dataGrid.CurrentItem = row;
                            dataGrid.SelectedItem = row;
                            dataGrid.ScrollIntoView(row);
                            break;
                        }
                    }
                }
            }
        }
    }
}
