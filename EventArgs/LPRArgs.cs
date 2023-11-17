using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;

namespace Illumine.LPR
{
    public class LPRArgs : EventArgs
    {
        public bool UseBackCameraToRetry { get; set; }

        private string _fileName = "";
        private DateTime _TimeStamp;
        private int _channelId;
        public List<PlateDataBundle> PlateList = new List<PlateDataBundle>();

        public string FileName => this._fileName;

        public DateTime TimeStamp => this._TimeStamp;

        public int ChannelId => this._channelId;

        public LPRArgs(string raw) => this.PlateList = this.Parse(raw);

        public LPRArgs(
          string fileName,
          DateTime timeStamp,
          int channelId,
          List<PlateDataBundle> plateList,
          bool UseBackCameraToRetry = false)
        {
            this.UseBackCameraToRetry = UseBackCameraToRetry;
            this._fileName = fileName;
            this._TimeStamp = timeStamp;
            this._channelId = channelId;
            this.PlateList = plateList;
        }

        private List<PlateDataBundle> Parse(string rawUDPData)
        {
            List<PlateDataBundle> plateDataBundleList = new List<PlateDataBundle>();
            string[] strArray1 = rawUDPData.Split('|');
            this._fileName = strArray1[0].Split(',')[0];
            this._channelId = int.Parse(Path.GetFileNameWithoutExtension(Path.GetDirectoryName(this._fileName)));
            this._TimeStamp = DateTime.ParseExact(Path.GetFileNameWithoutExtension(this._fileName), "yyyyMMddHHmmssfff", (IFormatProvider)CultureInfo.CurrentCulture);
            double num1 = 10.0;
            double num2 = 10.0;
            if (strArray1.Length > 1)
            {
                string[] strArray2 = strArray1[1].Split('/');
                int index = 0;
                if (index < strArray2.Length)
                {
                    string[] strArray3 = strArray2[index].Split(',');
                    string str = strArray3[0];
                    Image image = Image.FromFile(this._fileName);
                    Dictionary<string, Rectangle> dictionary = Container.Get<Dictionary<string, Rectangle>>();
                    Rectangle rectangle;
                    if (dictionary.ContainsKey(this._fileName))
                    {
                        rectangle = dictionary[this._fileName];
                        dictionary.Remove(this._fileName);
                    }
                    else
                    {
                        double num3 = (double)int.Parse(strArray3[1]);
                        double num4 = (double)int.Parse(strArray3[2]);
                        int x = (int)((double)image.Width * num3 / 100.0);
                        int y = (int)((double)image.Height * num4 / 100.0);
                        int width = (int)((double)image.Width * num1 / 100.0);
                        int height = (int)((double)image.Height * num2 / 100.0);
                        rectangle = new Rectangle(new Point(x, y) - new Size(width / 2, height / 2), new Size(width, height));
                    }
                    plateDataBundleList.Add(new PlateDataBundle()
                    {
                        PlateNumber = str,
                        Rectangle = rectangle
                    });
                }
            }
            return plateDataBundleList;
        }
    }

    public class LPRCameraArgs : EventArgs
    {
        public bool UseBackCameraToRetry { get; set; }

        public string PlateNumber { get; set; }

        public Rectangle PlateFrame { get; set; }

        public Image BigImage { get; set; }

        public int ChannelId { get; set; }

        public LPRCameraArgs(string plateNumber, Rectangle plateFrame, byte[] imageArray, int channelId, bool UseBackCameraToRetry = false)
        {
            this.UseBackCameraToRetry = UseBackCameraToRetry;

            ChannelId = channelId;
            PlateNumber = plateNumber;

            ImageQuality imageQuality = Container.Get<LPRSetting>().ImageQuality;
            Dictionary<ImageQuality, Size> dictionary = new Dictionary<ImageQuality, Size>()
            {
                {
                ImageQuality._1080P,
                new Size(1920, 1080)
                },
                {
                ImageQuality._720P,
                new Size(1280, 720)
                },
                {
                ImageQuality._4CIF,
                new Size(640, 480)
                }
            };

            using (MemoryStream memoryStream = new MemoryStream(imageArray))
            {
                BigImage = Image.FromStream(memoryStream);
            }

            if (imageQuality != ImageQuality.Default)
            {
                Size newSize = dictionary[imageQuality];
                if (plateFrame == new Rectangle())
                {
                    PlateFrame = new Rectangle(new Point(), newSize);
                }
                else
                {
                    int x = (int)Math.Round(plateFrame.X * (newSize.Width / (decimal)BigImage.Width));
                    int y = (int)Math.Round(plateFrame.Y * (newSize.Height / (decimal)BigImage.Height));
                    int width = (int)Math.Round(plateFrame.Width * (newSize.Width / (decimal)BigImage.Width));
                    int height = (int)Math.Round(plateFrame.Height * (newSize.Height / (decimal)BigImage.Height));
                    PlateFrame = new Rectangle(x, y, width, height);
                }
                BigImage = new Bitmap(BigImage, newSize);
            }
        }
    }
}
