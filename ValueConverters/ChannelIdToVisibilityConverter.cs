using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class ChannelIdToVisibilityConverter : BaseValueConverter<ChannelIdToVisibilityConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            bool flag = false;
            if (value is int ChannelId)
                flag = ChannelService.GetEnabled(ChannelId);
            return (object)(Visibility)(flag ? 0 : 1);
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
