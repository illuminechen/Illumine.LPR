using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;

namespace Illumine.LPR
{
    public class SearchValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            DataGridRow row = values[0] as DataGridRow;
            string searchText = values[1] as string;

            if (!string.IsNullOrEmpty(searchText) && row != null)
            {
                var vip = row.Item as VipViewModel;
                if(vip != null)
                    return vip.PlateNumber.ToLower().Contains(searchText.ToLower()) || vip.Name.ToLower().Contains(searchText.ToLower());
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
