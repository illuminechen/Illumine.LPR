using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class WidthToHeightConverter : BaseValueConverter<WidthToHeightConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (!(value is double width))
                return value;
            
            double ratio = 0.8;

            if (parameter is double r)
                ratio = r;

            return width * ratio;
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
