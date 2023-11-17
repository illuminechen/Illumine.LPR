using System;
using System.Globalization;

namespace Illumine.LPR
{
    public class RecordViewModelToSnapshotInfoTitleConverter :
      BaseValueConverter<RecordViewModelToSnapshotInfoTitleConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            string str = "";
            if (value is RecordViewModel vm)
            {
                SnapshotInfoViewModel viewModel = InfoService.GetViewModel(vm);
                str = viewModel == null ? "" : viewModel.Title;
            }
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
