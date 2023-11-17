using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Illumine.LPR
{
    public class GroupVMToDisplayString : BaseValueConverter<GroupVMToDisplayString>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                return null;
            if (!(value is ObservableCollection<GroupViewModel> list))
                return null;

            return new string[] { "無" }.Concat(list.Select(x => x.Id + ":" + x.GroupName));
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
