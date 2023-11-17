using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class ChannelIdToEntryConverter : BaseValueConverter<ChannelIdToEntryConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            string str = "";
            if (value is int ChannelId)
                str = ChannelService.GetEntryName(ChannelId);
            return (object)str;
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
