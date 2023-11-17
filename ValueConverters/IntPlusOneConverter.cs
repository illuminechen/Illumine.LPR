using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class IntPlusOneConverter : BaseValueConverter<IntPlusOneConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return (object)((int)value+1);
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
