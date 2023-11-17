using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class ChannelIdValidationConverter : BaseValueConverter<ChannelIdValidationConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return value is int num ? (object)(num > 0 & num <= 4) : (object)false;
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
