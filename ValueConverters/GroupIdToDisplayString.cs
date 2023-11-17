using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Illumine.LPR
{
    public class GroupIdToDisplayString : BaseValueConverter<GroupIdToDisplayString>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                return "無";
            if (!int.TryParse(value.ToString(), out int id))
                return "無";
            if (id == 0)
                return "無";

            var data = GroupDataService.GetData(id);
            if (data == null)
                return "無";

            return data.Id + ":" + data.GroupName;
        }

        public override object ConvertBack(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (value == null)
                return 0;
            var cs = value.ToString().Split(':')[0];
            if (!int.TryParse(cs, out int id))
                return 0;
            return id;
        }
    }
}
