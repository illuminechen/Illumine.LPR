using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Illumine.LPR
{
    public partial class NumericUpDown : UserControl, IComponentConnector
    {
        public static readonly DependencyProperty MaxValueProperty = DependencyProperty.Register(nameof(MaxValue), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata((object)100, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MaxValueChanged)));
        public static readonly DependencyProperty MinValueProperty = DependencyProperty.Register(nameof(MinValue), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata((object)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(MinValueChanged)));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericUpDown), new FrameworkPropertyMetadata((object)0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(ValueChanged)));

        public NumericUpDown()
        {
            InitializeComponent();
        }

        public int MaxValue
        {
            get => (int)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        private static void MaxValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is NumericUpDown numericUpDown))
                return;
            int num = Math.Min((int)e.NewValue, numericUpDown.Value);
            numericUpDown.Value = num;
            numericUpDown.tb.Text = num.ToString();
        }

        public int MinValue
        {
            get => (int)GetValue(MinValueProperty);
            set => SetValue(MinValueProperty, value);
        }

        private static void MinValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is NumericUpDown numericUpDown))
                return;
            int num = Math.Max((int)e.NewValue, numericUpDown.Value);
            numericUpDown.Value = num;
            numericUpDown.tb.Text = num.ToString();
        }

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        private static void ValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is NumericUpDown numericUpDown))
                return;
            int num = Math.Max(Math.Min((int)e.NewValue, numericUpDown.MaxValue), numericUpDown.MinValue);
            numericUpDown.Value = num;
            numericUpDown.tb.Text = num.ToString();
        }

        private void ViewBox_MouseDown(object sender, MouseButtonEventArgs e) => tb.Focus();

        private void Up_Click(object sender, RoutedEventArgs e) => SetValue(ValueProperty, Value + 1);

        private void Down_Click(object sender, RoutedEventArgs e) => SetValue(ValueProperty, Value - 1);
    }
}
