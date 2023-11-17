using System;
using System.Globalization;
using System.Windows;

namespace Illumine.LPR
{
    public class CurrentPageeCheckConverter : BaseValueConverter<CurrentPageeCheckConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                return false;

            if (!(int.TryParse(value.ToString(), out var left)))
                return false;

            var right = Container.Get<PagingGirdItemsControlViewModel>().CurrentPageIndex;

            return left == right;
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
