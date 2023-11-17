using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class EqualityCheckConverter : BaseValueConverter<EqualityCheckConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            if (!(int.TryParse(value.ToString(), out var left)))
                return false;
            if (!(int.TryParse(parameter.ToString(), out var right)))
                return false;

            return left == right;
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
