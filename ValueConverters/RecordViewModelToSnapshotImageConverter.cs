using System;
using System.Drawing;
using System.Globalization;

namespace Illumine.LPR
{
    public class RecordViewModelToSnapshotImageConverter :
      BaseValueConverter<RecordViewModelToSnapshotImageConverter>
    {
        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            Image image = (Image)null;
            if (value is RecordViewModel viewModel)
            {
                if (viewModel != null)
                    image = viewModel.Image;
            }
            return (object)image;
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
