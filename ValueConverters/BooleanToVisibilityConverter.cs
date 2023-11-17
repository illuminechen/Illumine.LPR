using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class BooleanToVisibilityConverter : BaseValueConverter<BooleanToVisibilityConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return parameter == null ? (object)(Visibility)((bool)value ? 0 : 1) : (object)(Visibility)((bool)value ? 1 : 0);
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
