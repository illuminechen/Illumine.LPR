using System.Windows;
using System.Windows.Controls;

namespace Illumine.LPR
{
    public class HasNoTextProperty : BaseAttachedProperty<HasNoTextProperty, bool>
    {
        public static void SetValue(DependencyObject sender) => BaseAttachedProperty<HasNoTextProperty, bool>.SetValue(sender, ((PasswordBox)sender).SecurePassword.Length == 0);
    }
}
