
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class UniformGirdItemsControl : ItemsControl
    {
        public UniformGirdItemsControl()
        {
            FrameworkElementFactory factory = new FrameworkElementFactory(typeof(UniformGrid), "UniformGrid");
            Binding bindingColumns = new Binding("UniformGirdColumns")
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
            };
            Binding bindingRows = new Binding("UniformGirdRows")
            {
                Mode = BindingMode.OneWay,
                RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, GetType(), 1)
            };
            factory.SetBinding(UniformGrid.ColumnsProperty, bindingColumns);
            factory.SetBinding(UniformGrid.RowsProperty, bindingRows);

            ItemsPanel = new ItemsPanelTemplate(factory);
        }

        #region UniformGrid number of Columns and Rows control
        public int UniformGirdColumns
        {
            get { return (int)GetValue(UniformGirdColumnsProperty); }
            set { SetValue(UniformGirdColumnsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UniformGirdColumns.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UniformGirdColumnsProperty =
            DependencyProperty.Register("UniformGirdColumns", typeof(int), typeof(UniformGirdItemsControl), new PropertyMetadata(2));

        public int UniformGirdRows
        {
            get { return (int)GetValue(UniformGirdRowsProperty); }
            set { SetValue(UniformGirdRowsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for UniformGirdRows.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UniformGirdRowsProperty =
            DependencyProperty.Register("UniformGirdRows", typeof(int), typeof(UniformGirdItemsControl), new PropertyMetadata(2));

        //public double MinColumnWidth
        //{
        //    get { return (double)GetValue(MinColumnWidthProperty); }
        //    set { SetValue(MinColumnWidthProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MinColumnWidth.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MinColumnWidthProperty =
        //    DependencyProperty.Register("MinColumnWidth", typeof(double), typeof(UniformGirdItemsControl),
        //        new PropertyMetadata(0d, OnDependencyPropertyChanged));

        //public double MinRowHeight
        //{
        //    get { return (double)GetValue(MinRowHeightProperty); }
        //    set { SetValue(MinRowHeightProperty, value); }
        //}

        //// Using a DependencyProperty as the backing store for MinRowHeight.  This enables animation, styling, binding, etc...
        //public static readonly DependencyProperty MinRowHeightProperty =
        //    DependencyProperty.Register("MinRowHeight", typeof(double), typeof(UniformGirdItemsControl),
        //        new PropertyMetadata(0d, OnDependencyPropertyChanged));

        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Orientation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register("Orientation", typeof(Orientation), typeof(UniformGirdItemsControl),
                new PropertyMetadata(Orientation.Horizontal, OnDependencyPropertyChanged));

        private static void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (UniformGirdItemsControl)d;
            itemsControl.OnSizeChanged();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            OnSizeChanged();
            base.OnRenderSizeChanged(sizeInfo);
        }

        private void OnSizeChanged()
        {
            //if (ActualWidth == 0 || ActualHeight == 0) return;
            //UniformGirdColumns = (MinColumnWidth != 0) ? Math.Max((int)Math.Floor(ActualWidth / MinColumnWidth), 1) : 1;
            //UniformGirdRows = (MinRowHeight != 0) ? Math.Max((int)Math.Floor(ActualHeight / MinRowHeight), 1) : 1;
            OnDataChanged();
        }
        #endregion

        public IEnumerable MainItemsSource
        {
            get { return (IEnumerable)GetValue(MainItemsSourceProperty); }
            set { SetValue(MainItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MainItemsSourceProperty =
            DependencyProperty.Register("MainItemsSource", typeof(IEnumerable), typeof(UniformGirdItemsControl),
                new PropertyMetadata(null, OnMainItemsSourceChanged));

        private static void OnMainItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var itemsControl = (UniformGirdItemsControl)d;
            if (e.OldValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.OldValue).CollectionChanged -= itemsControl.UniformGirdItemsControl_CollectionChanged;
            }
            if (e.NewValue is INotifyCollectionChanged)
            {
                ((INotifyCollectionChanged)e.NewValue).CollectionChanged += itemsControl.UniformGirdItemsControl_CollectionChanged;
            }

            itemsControl.OnDataChanged();
        }

        private void UniformGirdItemsControl_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDataChanged();
        }

        public int CurrentPageIndex
        {
            get { return (int)GetValue(CurrentPageIndexProperty); }
            set { SetValue(CurrentPageIndexProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinColumnWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentPageIndexProperty =
            DependencyProperty.Register("CurrentPageIndex", typeof(int), typeof(UniformGirdItemsControl),
                new PropertyMetadata(1, OnDependencyPropertyChanged));

        //private int _pageIndex;
        //private bool _isEnd;
        //private RelayCommand _previousPageCommand;
        //private RelayCommand _nextPageCommand;

        private void OnDataChanged()
        {
            if (MainItemsSource == null) return;
            var cells = UniformGirdColumns * UniformGirdRows;
            var enumerable = (MainItemsSource.OfType<object>())?.ToArray();
            var itemsSource = enumerable.Skip((CurrentPageIndex -1)* cells).Take(cells);        

            if (Orientation == Orientation.Vertical)
            {
                var finished = false;
                var itemsSourceArray = itemsSource.ToArray();
                var reorderedItemsSource = new List<object>();
                for (var i = 0; i < UniformGirdRows; i++)
                {
                    for (var j = 0; j < UniformGirdColumns; j++)
                    {
                        var arrayIndex = i + j * UniformGirdRows;
                        reorderedItemsSource.Add((arrayIndex < itemsSource.Count()) ? itemsSourceArray[j * UniformGirdRows + i] : null);
                        if (arrayIndex + 1 == itemsSource.Count())
                        {
                            finished = true;
                            break;
                        }
                    }
                    if (finished) break;
                }
                ItemsSource = reorderedItemsSource;
            }
            else
            {
                ItemsSource = itemsSource;
            }
        }

        //public ICommand PreviousPageCommand => _previousPageCommand ?? (_previousPageCommand = new RelayCommand(() =>
        //{
        //    _pageIndex--;
        //    OnDataChanged();
        //}, () => _pageIndex > 0));

        //public ICommand NextPageCommand => _nextPageCommand ?? (_nextPageCommand = new RelayCommand(() =>
        //{
        //    _pageIndex++;
        //    OnDataChanged();
        //}, () => !_isEnd));
    }
}
