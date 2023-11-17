using Illumine.LPR.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Illumine.LPR
{
    public class CameraViewerViewModel : BaseViewModel
    {
        public Action<object, LPRArgs> LPRCallBack;
        public IntPtr camId = IntPtr.Zero;
        public IntPtr backCamId = IntPtr.Zero;
        private object lockObj = new object();
        private CancellationTokenSource cts = new CancellationTokenSource();

        public ChannelViewModel ChannelViewModel => Container.Get<ChannelViewModel>(this.ChannelId);

        public bool FileWatching { get; set; }

        public bool IsPlaying { get; set; }

        public bool IsValid { get; set; }

        public bool IsConnecting { get; set; }

        public int ChannelId { get; set; } = -1;

        public IntPtr PlayingCameraId { get; set; } = IntPtr.Zero;

        public bool ShowControl => File.Exists("./Debug.flg");

        public bool OpenDoor()
        {
            if (this.camId == IntPtr.Zero)
                return false;

            LogHelper.Log("Open Door");

            return CameraServiceFactory.Create(this.ChannelViewModel.CameraType).OpenDoor(this.camId);
        }

        public void SendText(string text)
        {
            //int pos = 0;
            //for (int i = 0; i < 4; i++)
            //{
            //    string str = (text.Length < pos + 4) ? text.Substring(pos) : text.Substring(pos, 4);
            //    pos += 4;

            byte[] tdata = Encoding.UTF8.GetBytes(text);

            LogHelper.Log("SendText:" + text);
            byte[] msg = new byte[25 + tdata.Length + 2];
            byte[] cmd = new byte[] { 0x00, 0x64, 0xff, 0xff, 0x62, (byte)((tdata.Length+19) & 0xff), /*(byte)i*/0x00, 0x15, 0x01, 0x00, 0x05, 0x00, 0x01, 0x03, 0x00, 0xFF, 0x00, 0x00, 0x00,
                0x00, 0x00, 0x00, 0x00, (byte)((tdata.Length) & 0xff), 0x00};

            cmd.CopyTo(msg, 0);
            tdata.CopyTo(msg, cmd.Length);

            var CRC = CrcHelper.MB_CRC16(ref msg, 25 + tdata.Length);
            msg[25 + tdata.Length] = (byte)(CRC & 0xff);
            msg[25 + tdata.Length + 1] = (byte)((CRC >> 8) & 0xff);

            //LogHelper.Log(string.Join(" ", msg.Select(x => x.ToString("X2"))));

            CameraServiceFactory.Create(this.ChannelViewModel.CameraType).Send485(this.camId, msg);
            //}
        }


        public ICommand DisconnectCommand { get; set; }

        public ICommand PlayCommand { get; set; }

        public ICommand StopCommand { get; set; }

        public ICommand RefreshCommand { get; set; }

        public ICommand TestCommand { get; set; }

        public ICommand TriggerCommand { get; set; }

        public ICommand OpenDoorCommand { get; set; }

        public ICommand FakeETagCommand { get; set; }

        public ICommand TestLEDCommand { get; set; }
        public ICommand SensorInCommand { get; set; }
        public ICommand SensorOutCommand { get; set; }
        public ICommand FakeSensorInCommand { get; set; }
        public ICommand FakeSensorOutCommand { get; set; }

        public CameraViewerViewModel()
        {
            this.DisconnectCommand = new RelayCommand(new Action(this.Disconnect));
            this.PlayCommand = new RelayCommand(new Action(this.Play));
            this.StopCommand = new RelayCommand(new Action(this.Stop));
            this.RefreshCommand = new RelayCommand(new Action(this.Refresh));
            this.TestCommand = new RelayCommand(new Action(this.Test));
            this.TriggerCommand = new RelayCommand(new Action(TriggerFunction));
            this.OpenDoorCommand = new RelayCommand(new Action(this.Open));
            this.FakeETagCommand = new RelayCommand(new Action(this.FakeETag));
            this.TestLEDCommand = new RelayCommand(new Action(this.TestLED));
            this.SensorInCommand = new RelayCommand(new Action(this.SensorIn));
            this.SensorOutCommand = new RelayCommand(new Action(this.SensorOut));
            this.FakeSensorInCommand = new RelayCommand(new Action(this.FakeSensorIn));
            this.FakeSensorOutCommand = new RelayCommand(new Action(this.FakeSensorOut));
        }

        private void FakeSensorOut()
        {
            if (ChannelViewModel.SensorIp != "")
            {
                var modbus = Container.Get<string, ModbusTCPService>(ChannelViewModel.SensorIp);
                modbus.OnActuralLeave();
            }
        }

        private void FakeSensorIn()
        {
            if (ChannelViewModel.SensorIp != "")
            {
                var modbus = Container.Get<string, ModbusTCPService>(ChannelViewModel.SensorIp);
                modbus.OnActuralEnter();
            }
        }

        private void SensorOut()
        {
            if (ChannelViewModel.SensorIp != "")
            {
                var modbus = Container.Get<string, ModbusTCPService>(ChannelViewModel.SensorIp);
                modbus.Write(41, new ushort[] { 4 });
                Thread.Sleep(500);
                modbus.Write(41, new ushort[] { 12 });
                Thread.Sleep(1000);
                modbus.Write(41, new ushort[] { 8 });
                Thread.Sleep(500);
                modbus.Write(41, new ushort[] { 0 });
            }
        }

        private void SensorIn()
        {
            if (ChannelViewModel.SensorIp != "")
            {
                var modbus = Container.Get<string, ModbusTCPService>(ChannelViewModel.SensorIp);
                modbus.Write(41, new ushort[] { 1 });
                Thread.Sleep(500);
                modbus.Write(41, new ushort[] { 3 });
                Thread.Sleep(1000);
                modbus.Write(41, new ushort[] { 2 });
                Thread.Sleep(500);
                modbus.Write(41, new ushort[] { 0 });
            }
        }

        private void TestLED()
        {
            SendText("測試");
        }

        private void FakeETag()
        {
            this.ChannelViewModel.EtagReaderConnecting = true;
            SetETagNumber(this.ChannelViewModel, "0000100001");
        }

        private async void SetETagNumber(ChannelViewModel vm, string eTagNumber)
        {
            vm.EtagNumber = eTagNumber;
            await Task.Delay(Container.Get<LPRSetting>().ETagWaitingTime);
            vm.EtagNumber = "";
        }
        public void Disconnect()
        {
            Stop();
            CameraViewerViewModel me = this;
            CameraServiceFactory.Create(ChannelViewModel.CameraType).Disconnect(ref me);
        }

        private void TriggerFunction()
        {
            CameraServiceFactory.Create(ChannelViewModel.CameraType).Trigger(camId, ChannelId);
        }

        private void Open() => this.OpenDoor();

        private void Test()
        {
            DateTime now = DateTime.Now;
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Image")))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Image"));
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, "Image", this.ChannelId.ToString())))
                Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "Image", this.ChannelId.ToString()));
            string str = Path.Combine(Environment.CurrentDirectory, "Image", this.ChannelId.ToString(), now.ToString("yyyyMMddHHmmssfff") + ".jpg");
            Rectangle rectangle = new Rectangle(300, 300, 200, 100);
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
            Point location = new Point(rectangle.Left, rectangle.Top);
            Point point = new Point(rectangle.Right, rectangle.Bottom);
            Size size = new Size(point.X - location.X, point.Y - location.Y);
            if (imageQuality == ImageQuality.Default)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Resources.Test100.Save((Stream)memoryStream, ImageFormat.Jpeg);
                    using (FileStream fileStream = new FileStream(str, FileMode.Create, FileAccess.Write))
                        memoryStream.WriteTo((Stream)fileStream);
                }
            }
            else
            {
                Size newSize = dictionary[imageQuality];
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    Resources.Test100.Save((Stream)memoryStream, ImageFormat.Jpeg);
                    Image original = Image.FromStream((Stream)memoryStream);
                    location.X = (int)Math.Round((Decimal)location.X * ((Decimal)newSize.Width / (Decimal)original.Width));
                    location.Y = (int)Math.Round((Decimal)location.Y * ((Decimal)newSize.Height / (Decimal)original.Height));
                    size.Width = (int)Math.Round((Decimal)size.Width * ((Decimal)newSize.Width / (Decimal)original.Width));
                    size.Height = (int)Math.Round((Decimal)size.Height * ((Decimal)newSize.Height / (Decimal)original.Height));
                    new Bitmap(original, newSize).Save(str, ImageFormat.Jpeg);
                }
            }
            if (Container.Get<LPRSetting>().LPRMode == LPRMode.UDPServer)
            {
                Container.Get<Dictionary<string, Rectangle>>().Put<string, Rectangle>(str, new Rectangle(location, size));
                using (UDPClient udpClient = new UDPClient(Container.Get<LPRSetting>().Ip, Container.Get<LPRSetting>().HostPort))
                {
                    LogHelper.Log("SendTestUDP");
                    udpClient.Send(str + "|TEST100,50,50");
                }
            }
            else
            {
                List<PlateDataBundle> plateList = new List<PlateDataBundle>();
                plateList.Add(new PlateDataBundle()
                {
                    PlateNumber = "#######",
                    Rectangle = new Rectangle(location, size)
                });
                LogHelper.Log("SendTestCallback");
                Action<object, LPRArgs> lprCallBack = this.LPRCallBack;
                if (lprCallBack == null)
                    return;
                lprCallBack((object)this, new LPRArgs(str, now, this.ChannelId, plateList));
            }
        }

        private void Refresh()
        {
            this.Stop();
            this.Play();
        }

        public void Stop()
        {
            if (!this.IsPlaying)
                return;
            LogHelper.Log("Stop s" + camId);
            CameraServiceFactory.Create(ChannelViewModel.CameraType).StopVideo(camId);
            this.IsPlaying = false;
        }

        public void Play()
        {
            try
            {
                InitiateCameraAsync();
            }
            catch (Exception ex)
            {
                LogHelper.Log("Camera Play", ex);
            }
        }

        private void CheckValid()
        {
            IsValid = false;
            var me = this;
            CameraServiceFactory.Create(ChannelViewModel.CameraType).CheckValid(ref me);
            if (!IsValid)
            {
                Disconnect();
                LogHelper.Log(camId + " invalid disconnect");
            }
        }

        private async void InitiateCameraAsync()
        {
            CameraViewerViewModel cameraViewerViewModel = this;
            if (cameraViewerViewModel.IsPlaying)
                return;
            Action action = new Action(() =>
            {
                bool successed = CameraServiceFactory.Create(this.ChannelViewModel.CameraType).Connect(ref cameraViewerViewModel);
                if (!successed)
                    return;
                CameraServiceFactory.Create(this.ChannelViewModel.CameraType).SetCallback(this, AuthenticationService.CameraCallback);

                LogHelper.Log("ConnCamera", string.Format("Success: id:{0}", (object)cameraViewerViewModel.camId));
                if (ChannelViewModel.BackUrl != "")
                {
                    successed = CameraServiceFactory.Create(this.ChannelViewModel.BackCameraType).BackConnect(ref cameraViewerViewModel);
                    if (!successed)
                        return;
                    CameraServiceFactory.Create(this.ChannelViewModel.BackCameraType).SetBackCallback(this, AuthenticationService.CameraBackCallback);

                    LogHelper.Log("ConnBackCamera", string.Format("Success: id:{0}", (object)cameraViewerViewModel.backCamId));
                }

            });

            cameraViewerViewModel.cts = new CancellationTokenSource();
            cameraViewerViewModel.IsConnecting = true;
            try
            {
                if (cameraViewerViewModel.ChannelViewModel.InitialTimeout > 0)
                {
                    Task delaytask = Task.Delay(cameraViewerViewModel.ChannelViewModel.InitialTimeout, cameraViewerViewModel.cts.Token);
                    if (await Task.WhenAny(delaytask, Task.Run(action, cameraViewerViewModel.cts.Token)) == delaytask)
                        throw new Exception("failed");
                    delaytask = null;
                }
                else
                    await Task.Run(action);
                cameraViewerViewModel.IsPlaying = true;
            }
            catch (Exception ex)
            {
                LogHelper.Log("ConnCamera", ex);
                cameraViewerViewModel.IsPlaying = false;
            }
            finally
            {
                cameraViewerViewModel.IsConnecting = false;
            }

            CheckValid();

            if (IsValid && camId != IntPtr.Zero)
            {
                PlayingCameraId = camId;
            }
        }
    }
}
