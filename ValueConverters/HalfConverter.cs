using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class HalfConverter : BaseValueConverter<HalfConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (!(value is double v))
                return value;

            if (parameter == null)
                return v / 2;

            if (!int.TryParse(parameter.ToString(), out int offset))
                return v / 2;

            return (v + offset) / 2;
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
