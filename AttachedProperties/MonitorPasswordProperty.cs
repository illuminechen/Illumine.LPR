using System.Windows;
using System.Windows.Controls;

namespace Illumine.LPR
{
    public class MonitorPasswordProperty : BaseAttachedProperty<MonitorPasswordProperty, bool>
    {
        public override void OnValueChanged(
          DependencyObject sender,
          DependencyPropertyChangedEventArgs e)
        {
            if (!(sender is PasswordBox d))
                return;
            d.PasswordChanged -= new RoutedEventHandler(this.PasswordBox_PasswordChanged);
            if (!(bool)e.NewValue)
                return;
            BaseAttachedProperty<HasNoTextProperty, bool>.SetValue((DependencyObject)d, d.SecurePassword.Length == 0);
            d.PasswordChanged += new RoutedEventHandler(this.PasswordBox_PasswordChanged);
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e) => HasNoTextProperty.SetValue((DependencyObject)sender);
    }
}
