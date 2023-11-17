using System.Windows;

namespace Illumine.LPR
{
    /// <summary>
    /// ProgressDialog.xaml 的互動邏輯
    /// </summary>
    public partial class ProgressDialog : Window
    {
        public bool ForcingClose { get; set; } = false;
        public ProgressDialog()
        {
            InitializeComponent();
        }

        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Caption.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(ProgressDialog), new PropertyMetadata("", OnCaptionChanged));

        private static void OnCaptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressDialog progressDialog))
                return;

            progressDialog.caption.Text = (string)e.NewValue;
        }

        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Maximum.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", typeof(int), typeof(ProgressDialog), new PropertyMetadata(0, OnMaximumChanged));

        private static void OnMaximumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressDialog progressDialog))
                return;

            progressDialog.pg.Maximum = (int)e.NewValue;
            progressDialog.step.Text = $"{e.NewValue}/{progressDialog.Maximum}";
        }

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(ProgressDialog), new PropertyMetadata(0, OnValueChanged));

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is ProgressDialog progressDialog))
                return;

            progressDialog.pg.Value = (int)e.NewValue;
            progressDialog.step.Text = $"{e.NewValue}/{progressDialog.Maximum}";
        }

        public void End()
        {
            pg.Value = pg.Maximum;
            step.Text = $"{pg.Value}/{pg.Maximum}";
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ForcingClose)
            {

            }
            else
            {
                this.Hide();
                e.Cancel = true;
            }
        }


    }
}
