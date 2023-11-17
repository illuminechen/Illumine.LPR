using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class FilterModeToCheckedConverter : BaseValueConverter<FilterModeToCheckedConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (!(value is int num))
                return (object)false;
            int result;
            return !int.TryParse(parameter.ToString(), out result) ? (object)false : (object)(num == result);
        }

        public override object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null || parameter == null)
                return (object)null;
            return (bool)value ? (object)parameter.ToString() : (object)null;
        }
    }
}
