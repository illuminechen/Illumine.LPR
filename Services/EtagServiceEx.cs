using Illumine.LPR.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using static Illumine.LPR.EtagService;
using static System.Windows.Forms.AxHost;

namespace Illumine.LPR
{
    public static class EtagServiceEx
    {
        public static Dictionary<int, bool> isConnected = new Dictionary<int, bool>();
        static System.Threading.Timer timer;

        public static bool Init()
        {
            try
            {
                foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                {
                    if (!string.IsNullOrWhiteSpace(channelDataModel.EtagReaderExIp))
                    {
                        bool b = InitReader(channelDataModel.Id, channelDataModel.EtagReaderExIp);
                        LogHelper.Log("eTagEx init:" + b);

                        var chvm = Container.Get<ChannelViewModel>(channelDataModel.Id);
                        chvm.EtagReaderConnecting = b;

                        Container.Put(Task.Run(() =>
                        {
                            while (isConnected[channelDataModel.Id])
                            {
                                try
                                {
                                    var socket = Container.Get<ETagSocket>(channelDataModel.Id);

                                    //43 54 00 2C 00 45 01
                                    //C4 DD 93 8E 17 01 23
                                    //02
                                    //0F 01 02 E2 00 20 75 60 10 01 82 04 80 E0 64 33
                                    //0F 01 03 E3 00 20 75 60 10 01 82 04 80 E0 64 34
                                    //07
                                    byte[] buffer = new byte[1024];
                                    int bytesRead = socket.Receive(buffer);
                                    if (bytesRead > 16)
                                    {
                                        int num = buffer[14];
                                        int sitecode = buffer[22] * 256 + buffer[23];
                                        int cardcode = buffer[24] * 256 + buffer[25];

                                        Task.Run(() =>
                                        {
                                            var cvvm = Container.Get<CameraViewerViewModel>(channelDataModel.Id);
                                            var camera = Container.Get<Camera>(channelDataModel.Id);

                                            string etag = sitecode.ToString("00000") + cardcode.ToString("00000");

                                            if (Container.Get<LPRSetting>().ETagMode != ETagMode.Hybrid)
                                                return;

                                            chvm.EtagNumber = etag;
                                            CameraServiceFactory.Create(chvm.CameraType).Trigger(cvvm.camId, cvvm.ChannelId);

                                            //var data = VipDataService.TryGetPlateByEtag(etag);
                                            //if (data != null)
                                            //{
                                            //    DateTime now = DateTime.Now;
                                            //    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Image")))
                                            //        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Image"));
                                            //    if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Image", channelDataModel.Id.ToString())))
                                            //        Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Image", channelDataModel.Id.ToString()));
                                            //    string str = Path.Combine(Environment.CurrentDirectory, "Image", channelDataModel.Id.ToString(), now.ToString("yyyyMMddHHmmssfff") + ".jpg");
                                            //    Rectangle rectangle = new Rectangle(300, 300, 200, 100);
                                            //    Point location = new Point(rectangle.Left, rectangle.Top);
                                            //    Point point = new Point(rectangle.Right, rectangle.Bottom);
                                            //    Size size = new Size(point.X - location.X, point.Y - location.Y);
                                            //    ImageQuality imageQuality = Container.Get<LPRSetting>().ImageQuality;

                                            //    Dictionary<ImageQuality, Size> dictionary = new Dictionary<ImageQuality, Size>()
                                            //    {
                                            //        {
                                            //            ImageQuality._1080P,
                                            //            new Size(1920, 1080)
                                            //        },
                                            //        {
                                            //            ImageQuality._720P,
                                            //            new Size(1280, 720)
                                            //        },
                                            //        {
                                            //            ImageQuality._4CIF,
                                            //            new Size(640, 480)
                                            //        }
                                            //    };

                                            //    if (imageQuality == ImageQuality.Default)
                                            //    {
                                            //        using (MemoryStream memoryStream = new MemoryStream())
                                            //        {
                                            //            camera.Snapshot().Save((Stream)memoryStream, ImageFormat.Jpeg);
                                            //            using (FileStream fileStream = new FileStream(str, FileMode.Create, FileAccess.Write))
                                            //                memoryStream.WriteTo((Stream)fileStream);
                                            //        }
                                            //    }
                                            //    else
                                            //    {
                                            //        Size newSize = dictionary[imageQuality];
                                            //        using (MemoryStream memoryStream = new MemoryStream())
                                            //        {
                                            //            camera.Snapshot().Save((Stream)memoryStream, ImageFormat.Jpeg);
                                            //            System.Drawing.Image original = System.Drawing.Image.FromStream((Stream)memoryStream);
                                            //            location.X = (int)Math.Round((Decimal)location.X * ((Decimal)newSize.Width / (Decimal)original.Width));
                                            //            location.Y = (int)Math.Round((Decimal)location.Y * ((Decimal)newSize.Height / (Decimal)original.Height));
                                            //            size.Width = (int)Math.Round((Decimal)size.Width * ((Decimal)newSize.Width / (Decimal)original.Width));
                                            //            size.Height = (int)Math.Round((Decimal)size.Height * ((Decimal)newSize.Height / (Decimal)original.Height));
                                            //            new Bitmap(original, newSize).Save(str, ImageFormat.Jpeg);
                                            //        }
                                            //    }

                                            //    List<PlateDataBundle> plateList = new List<PlateDataBundle>();
                                            //    plateList.Add(new PlateDataBundle()
                                            //    {
                                            //        PlateNumber = data.PlateNumber,
                                            //        Rectangle = new Rectangle(location, size)
                                            //    });
                                            //    LogHelper.Log("Trigger From Etag");

                                            //    if (cvvm.LPRCallBack == null)
                                            //        return;
                                            //    cvvm.LPRCallBack(cvvm, new LPRArgs(str, now, channelDataModel.Id, plateList));
                                            //}

                                        });
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.ToString();
                                }
                            }
                        }), channelDataModel.Id);
                    }
                }


                TimerCallback callback = new TimerCallback((state) =>
                {
                    foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                    {
                        ChannelViewModel channelViewModel = Container.Get<ChannelViewModel>(channelDataModel.Id);
                        if (!string.IsNullOrWhiteSpace(channelViewModel.EtagReaderExIp))
                        {
                            if (!channelViewModel.EtagReaderConnecting)
                            {
                                var oritask = Container.Get<Task>(channelDataModel.Id);
                                if (oritask != null)
                                    oritask.Dispose();

                                bool b = InitReader(channelDataModel.Id, channelDataModel.EtagReaderExIp);
                                var chvm = Container.Get<ChannelViewModel>(channelDataModel.Id);
                                chvm.EtagReaderConnecting = b;
                                if (b)
                                {
                                    var task = Task.Run(() =>
                                    {
                                        while (isConnected[channelDataModel.Id])
                                        {
                                            var socket = Container.Get<ETagSocket>(channelDataModel.Id);

                                            //43 54 00 2C 00 45 01
                                            //C4 DD 93 8E 17 01 23
                                            //02
                                            //0F 01 02 E2 00 20 75 60 10 01 82 04 80 E0 64 33
                                            //0F 01 03 E3 00 20 75 60 10 01 82 04 80 E0 64 34
                                            //07
                                            byte[] buffer = new byte[1024];
                                            int bytesRead = socket.Receive(buffer);
                                            if (bytesRead > 16)
                                            {
                                                int num = buffer[14];
                                                int sitecode = buffer[27] * 256 + buffer[28];
                                                int cardcode = buffer[29] * 256 + buffer[30];

                                                Task.Run(() => SetETagNumber(chvm, sitecode.ToString("00000") + cardcode.ToString("00000")));
                                            }
                                        }
                                    });
                                    Container.Put(task, channelDataModel.Id);
                                }
                            }
                        }
                    }
                });

                //1.function 2.開關  3.等多久再開始  4.隔多久反覆執行
                timer = new System.Threading.Timer(callback, null, 5000, 5000);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
            return false;
        }

        private static bool InitReader(int id, string ip)
        {
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ip), 60000);
            ETagSocket socket = new ETagSocket(ipep);

            Container.Put(id, socket);

            try
            {
                socket.Connect(ipep);
                byte[] cmd = new byte[] { 0x53, 0x57, 0x00, 0x03, 0xFF, 0x10, 0x44 };
                socket.Send(cmd);

                byte[] buffer0 = new byte[17];
                int revc = socket.Receive(buffer0);

                socket.DeviceId = new byte[] { buffer0[9], buffer0[10], buffer0[11], buffer0[12], buffer0[13], buffer0[14], buffer0[15], buffer0[16] };

                byte[] cmd1 = new byte[] { 0x53, 0x57, 0x00, 0x03, 0xFF, 0x41, 0x13 };
                socket.Send(cmd1);
                byte[] buffer = new byte[8];
                socket.Receive(buffer);
                // 43 54 00 04 00 41 01 23
                if (
                   buffer[0] == 0x43 &&
                   buffer[1] == 0x54 &&
                   buffer[2] == 0x00 &&
                   buffer[3] == 0x04)
                {
                    isConnected[id] = true;

                    return true;
                }

            }
            catch
            {

            }
            return false;
        }

        private static async void SetETagNumber(ChannelViewModel vm, string eTagNumber)
        {
            vm.EtagNumber = eTagNumber;
            await Task.Delay(Container.Get<LPRSetting>().ETagWaitingTime);
            vm.EtagNumber = "";
        }

        public class ETagSocket : Socket
        {
            public byte[] DeviceId { get; set; } = new byte[0];
            public ETagSocket(IPEndPoint remoteServerEP) : base(remoteServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
            {

            }
        }
    }
}
