using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media.Imaging;

namespace Illumine.LPR
{
    public static class ImageHelper
    {
        public static BitmapImage ToBitmapImage(this Image bitmap)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                bitmap.Save((Stream)memoryStream, ImageFormat.Bmp);
                memoryStream.Position = 0L;
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = (Stream)memoryStream;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }
    }
}
