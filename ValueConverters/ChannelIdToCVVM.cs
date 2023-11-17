using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class ChannelIdToCVVM : BaseValueConverter<ChannelIdToCVVM>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!int.TryParse(value.ToString(), out int ChannelId))
                return null;

            return Container.Get<ChannelViewerViewModel>(ChannelId);
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
