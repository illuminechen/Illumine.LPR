using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class IntToTwoDigitsConverter : BaseValueConverter<IntToTwoDigitsConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return (object)((int)value).ToString("00");
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
