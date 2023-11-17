using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class EntryModeToVisibilityConverter : BaseValueConverter<EntryModeToVisibilityConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            return parameter == null ? (object)(Visibility)((EntryMode)value == EntryMode.Out ? 0 : 1) : (object)(Visibility)((EntryMode)value == EntryMode.In ? 1 : 0);
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
