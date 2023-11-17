using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Illumine.LPR
{
    public class ImagePtrToBrushConverter : BaseValueConverter<ImagePtrToBrushConverter>
    {
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public override object Convert(
          object value,
          Type targetType,
          object parameter,
          CultureInfo culture)
        {
            if (!(value is IntPtr num) || num == IntPtr.Zero)
                return (object)new SolidColorBrush(Color.FromArgb((byte)0, (byte)0, (byte)0, (byte)0));
            BitmapSource sourceFromHbitmap = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(num, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            sourceFromHbitmap.Freeze();
            ImagePtrToBrushConverter.DeleteObject(num);
            return (object)new ImageBrush((ImageSource)sourceFromHbitmap);
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
