using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class RecordViewModelToSnapshotInfoConverter : BaseValueConverter<RecordViewModelToSnapshotInfoConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            SnapshotInfoViewModel snapshotInfoViewModel = null;
            if (value is RecordViewModel vm)
                snapshotInfoViewModel = InfoService.GetViewModel(vm);
            return (object)snapshotInfoViewModel;
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
