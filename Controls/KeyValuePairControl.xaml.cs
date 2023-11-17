using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Illumine.LPR
{
    public partial class KeyValuePairControl : UserControl
    {
        public static readonly DependencyProperty LabelWidthProperty = DependencyProperty.Register(nameof(LabelWidth), typeof(GridLength), typeof(KeyValuePairControl), new PropertyMetadata((object)GridLength.Auto, new PropertyChangedCallback(KeyValuePairControl.LabelWidthChangeCallback)));
        public KeyValuePairControl()
        {
            InitializeComponent();
        }
        public GridLength LabelWidth
        {
            get => (GridLength)this.GetValue(KeyValuePairControl.LabelWidthProperty);
            set => this.SetValue(KeyValuePairControl.LabelWidthProperty, (object)value);
        }

        private static void LabelWidthChangeCallback(
          DependencyObject d,
          DependencyPropertyChangedEventArgs e)
        {
            if (!(d is KeyValuePairControl valuePairControl))
                return;
            try
            {
                valuePairControl.LabelColumnDefinition.Width = (GridLength)e.NewValue;
            }
            catch (Exception ex)
            {
                Debugger.Break();
                valuePairControl.LabelColumnDefinition.Width = GridLength.Auto;
            }
        }
    }
}
