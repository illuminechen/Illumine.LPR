using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class NullToVisibilityConverter : BaseValueConverter<NullToVisibilityConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return parameter == null ? (object)(Visibility)(value == null ? 0 : 2) : (object)(Visibility)(value == null ? 2 : 0);
        }

        public override object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
