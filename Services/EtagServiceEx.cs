using System;
using System.Collections.Generic;
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
        public static Dictionary<int, bool> isConnected;
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
                byte[] cmd = new byte[] { 0x53, 0x57, 0x00, 0x03, 0xFF, 0x10, 0x44, 0x00, 0x00 };
                var crc = CrcHelper.MB_CRC16(ref cmd, 7);
                cmd[7] = (byte)(crc & 0xff);
                cmd[8] = (byte)((crc >> 8) & 0xff);
                socket.Send(cmd);

                byte[] buffer0 = new byte[17];
                socket.Receive(buffer0);

                socket.DeviceId = new byte[] { buffer0[9], buffer0[10], buffer0[11], buffer0[12], buffer0[13], buffer0[14], buffer0[15], buffer0[16] };

                byte[] cmd1 = new byte[] { 0x53, 0x57, 0x00, 0x03, 0xFF, 0x41, 0x13, 0x00, 0x00 };
                var crc1 = CrcHelper.MB_CRC16(ref cmd, 7);
                cmd[7] = (byte)(crc1 & 0xff);
                cmd[8] = (byte)((crc1 >> 8) & 0xff);
                socket.Send(cmd1);
                byte[] buffer = new byte[8];
                socket.Receive(buffer);
                // 43 54 00 04 00 41 01 23
                if (
                   buffer[0] == 0x43 &&
                   buffer[1] == 0x54 &&
                   buffer[2] == 0x00 &&
                   buffer[3] == 0x04 &&
                   buffer[4] == 0x00 &&
                   buffer[5] == 0x41 &&
                   buffer[6] == 0x01 &&
                   buffer[7] == 0x23)
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
