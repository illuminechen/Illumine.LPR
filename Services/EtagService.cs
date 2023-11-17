using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Illumine.LPR
{
    public static class EtagService
    {
        static UdpClient udpClient = new UdpClient();
        static CancellationTokenSource cts;
        public static bool isConnected;
        public static IPEndPoint hostEP = new System.Net.IPEndPoint(IPAddress.Parse(Container.Get<LPRSetting>().HostIp), 1688);
        static Socket listener;
        static System.Threading.Timer timer;


        public static bool Init()
        {
            try
            {
                listener = new Socket(hostEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(hostEP);
                listener.Listen(32);
                isConnected = true;
                cts = new CancellationTokenSource();
                Task.Run(new Action(() =>
                {
                    while (isConnected)
                    {
                        Socket clientSocket = listener.Accept();

                        IPEndPoint clientInfo = (IPEndPoint)clientSocket.RemoteEndPoint;
                        IPEndPoint serverInfo = (IPEndPoint)listener.LocalEndPoint;

                        string ipString = clientInfo.Address.ToString();
                        var channel = Container.Get<List<ChannelDataModel>>().Find(x => x.EtagReaderIp == ipString);
                        int index = 0;
                        if (channel != null)
                        {
                            index = channel.Id;

                            var channelVM = Container.Get<ChannelViewModel>(index);

                            StateObject state = new StateObject();
                            state.dcuid = (byte)index;
                            state.workSocket = clientSocket;
                            clientSocket.BeginReceive(state._buffer, 0, StateObject.BufferSize, 0,
                                 new AsyncCallback((IAsyncResult ar) =>
                                 {
                                     state = (StateObject)ar.AsyncState;
                                     Socket handler = state.workSocket;
                                     IPEndPoint remoteEP = (IPEndPoint)state.workSocket.RemoteEndPoint;
                                     string ip = remoteEP.Address.ToString();
                                     var ch = Container.Get<List<ChannelDataModel>>().Find(x => x.EtagReaderIp == ip);
                                     var chVM = Container.Get<ChannelViewModel>(ch.Id);
                                     // Read data from the client socket. 
                                     int bytesRead = handler.EndReceive(ar);

                                     if (bytesRead >= 16 && state.func == 60 && MsgConstant.FormatRef.Find(x => x.index == state.subfunc).InfoMode == MsgObject.mode.sc)
                                     {
                                         Task.Run(() => SetETagNumber(chVM, state.SiteCode.ToString("00000") + state.CardCode.ToString("00000")));
                                     }

                                     handler.Shutdown(SocketShutdown.Both);
                                     handler.Close();
                                 }), state);
                        }
                    }
                }), cts.Token);

                foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                {
                    if (!string.IsNullOrWhiteSpace(channelDataModel.EtagReaderIp))
                    {
                        InitReader(channelDataModel.Id, channelDataModel.EtagReaderIp);
                    }
                }

                TimerCallback callback = new TimerCallback((state) =>
                {
                    foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                    {
                        ChannelViewModel channelViewModel = Container.Get<ChannelViewModel>(channelDataModel.Id);
                        if (!string.IsNullOrWhiteSpace(channelViewModel.EtagReaderIp))
                        {
                            if (!channelViewModel.EtagReaderConnecting)
                            {
                                if (udpfind(channelViewModel.EtagReaderIp))
                                {
                                    if (InitReader(channelViewModel.Id, channelViewModel.EtagReaderIp))
                                        channelViewModel.EtagReaderConnecting = true;
                                }
                            }
                            else
                            {
                                if (!udpfind(channelViewModel.EtagReaderIp))
                                {
                                    channelViewModel.EtagReaderConnecting = false;
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

        private static async void SetETagNumber(ChannelViewModel vm, string eTagNumber)
        {
            vm.EtagNumber = eTagNumber;
            await Task.Delay(Container.Get<LPRSetting>().ETagWaitingTime);
            vm.EtagNumber = "";
        }

        public static bool InitReader(int channelId, string ip)
        {
            //30303 連線確認
            if (!udpfind(ip, true))
                return false;
            try
            {
                CommandSocket socket = new CommandSocket(new IPEndPoint(IPAddress.Parse(ip), 5889));

                Container.Put(channelId, socket);
                if (!socket.Connect(true))
                    throw new Exception("setTCP(false)");

                //if (obj.disconnectedFlag)
                //    throw new Exception("disconnectedFlag");
                // 命令碼168:停止TCP(1689)連線，卡機訊息會存在DCU Queue中
                Console.WriteLine("[setTCP]");
                if (!socket.setTCP(false))
                    throw new Exception("setTCP(false)");
                // 命令碼36:Disable DCU要求下位模組回傳模組佇列筆數旗標
                Console.WriteLine("[setUpload]");
                if (!socket.setUpload(false))
                    throw new Exception("setUpload(false)");
                // 命令碼17:校正DCU日期時間
                Console.WriteLine("[setTime]");
                if (!socket.setTime())
                    throw new Exception("setTime()");

                // 命令碼30:啟動DCU polling下位模組
                Console.WriteLine("[setPolling]");
                if (!socket.setPolling(true))
                    throw new Exception("setPolling(true)");
                // 命令碼36:Enable DCU要求下位模組回傳模組佇列筆數旗標
                Console.WriteLine("[setUpload]");
                if (!socket.setUpload(true))
                    throw new Exception("setUpload(true)");
                // 命令碼168:Enable 啟動TCP(1689)
                Console.WriteLine("[setTCP]");
                if (!socket.setTCP(true))
                    throw new Exception("setTCP(true)");

                new iACSSocket(socket.cmdSocket, true).ClearUsers();
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex);
            }
            return false;
        }

        public static bool udpfind(string ip, bool first = false)
        {
            try
            {
                byte[] DiscoverMsg = Encoding.ASCII.GetBytes(first ? "Explore: Where is DCU?" : "Discovery: Where is DCU?");
                udpClient.Client.ReceiveTimeout = 500;
                IPEndPoint ipep = new IPEndPoint(IPAddress.Parse("255.255.255.255"), 30303);
                udpClient.Send(DiscoverMsg, DiscoverMsg.Length, new IPEndPoint(IPAddress.Parse(ip), 30303));
                string ReceiveString = Encoding.ASCII.GetString(udpClient.Receive(ref ipep));
                return !string.IsNullOrWhiteSpace(ReceiveString);
            }
            catch (Exception ex)
            { }
            return false;
        }

        //public static void callback(IAsyncResult ar)
        //{
        //    StateObject state;
        //    Socket handler = null;
        //    IPEndPoint remoteEP;
        //    int bytesRead;

        //    try
        //    {
        //        // Retrieve the state object and the handler socket from the asynchronous state object.
        //        state = (StateObject)ar.AsyncState;
        //        handler = state.workSocket;
        //        remoteEP = (IPEndPoint)state.workSocket.RemoteEndPoint;

        //        // Read data from the client socket. 
        //        bytesRead = handler.EndReceive(ar);

        //        if (state.func == 60 && MsgConstant.FormatRef.Find(x => x.index == state.subfunc).InfoMode == MsgObject.mode.sc)
        //        {
        //              state.SiteCode.ToString("00000") + state.CardCode.ToString("00000");
        //        }
        //        var msgobj = state.msgObj;

        //    }
        //    catch (Exception)
        //    {
        //        return;
        //    }
        //    finally
        //    {
        //        if (handler != null)
        //        {
        //            handler.Shutdown(SocketShutdown.Both);
        //            handler.Close();
        //        }
        //    }
        //}

        public class MsgObject
        {
            public MsgObject(int id, string info, string sinfo, mode m, playsound p)
            {
                index = id;
                Info = info;
                SubInfo = sinfo;
                InfoMode = m;
                Sound = p;
            }
            public int index;
            public string Info;
            public string SubInfo;
            public enum mode { ppw, dpw, sc, uc, onoff, press, n };
            public enum playsound { n, notify, Firealarm, Impassable, Duress, AuthenSuccess, Lockeypad, unlockeypad, ClosedOvertime, Forcedentry, Violentdamage, Authenfail, Anti_passback, disconnected, call, EmergencyCall, Connection, error };
            public mode InfoMode;
            public playsound Sound;
        }

        public static class MsgConstant
        {
            public static List<int> DIlist = new List<int>() { 22 };
            public static List<int> DOlist = new List<int>() { };
            public static List<MsgObject> FormatRef =
                new MsgObject[] {
                new MsgObject(0,"不可通行","一般(密碼)", MsgObject.mode.n, MsgObject.playsound.Impassable),
                new MsgObject(1,"不可通行","一般(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Impassable),
                new MsgObject(2,"脅迫","一般(反脅迫密碼)", MsgObject.mode.dpw, MsgObject.playsound.Duress),
                new MsgObject(3,"公用","一般(公用密碼)", MsgObject.mode.ppw, MsgObject.playsound.AuthenSuccess),
                new MsgObject(4,"無效","一般(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(5,"有效","一般(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(6,"鍵盤鎖住","", MsgObject.mode.n, MsgObject.playsound.n),
                new MsgObject(7,"鍵盤解鎖","", MsgObject.mode.n, MsgObject.playsound.n),
                new MsgObject(8,"無效","一般(讀卡或密碼)", MsgObject.mode.n, MsgObject.playsound.Authenfail),
                new MsgObject(9,"有效","一般(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(10,"有效","一般(密碼)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(12,"無效","一般(讀卡+密碼)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(13,"無效","一般(+密碼不符)", MsgObject.mode.uc, MsgObject.playsound.Authenfail),
                new MsgObject(14,"無效","一般(+卡號不符)", MsgObject.mode.onoff, MsgObject.playsound.Authenfail),
                new MsgObject(15,"有效","讀卡+密碼", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(16,"無效","進出(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(17,"無效","進出(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(18,"按鈕開門","", MsgObject.mode.press, MsgObject.playsound.AuthenSuccess),
                new MsgObject(19,"逾時未關門警報","", MsgObject.mode.n, MsgObject.playsound.ClosedOvertime),
                new MsgObject(20,"強行闖入警報","", MsgObject.mode.n, MsgObject.playsound.Forcedentry),
                new MsgObject(21,"有效","進出(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(22,"卡機拆除警報","", MsgObject.mode.n, MsgObject.playsound.Violentdamage),
                new MsgObject(23,"Relay變化","", MsgObject.mode.onoff, MsgObject.playsound.Violentdamage),
                new MsgObject(24,"不可通行","進出(密碼)", MsgObject.mode.n, MsgObject.playsound.Impassable),
                new MsgObject(25,"無效","違反進出(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(26,"有效","QRcode(讀卡)", MsgObject.mode.uc, MsgObject.playsound.Authenfail),
                new MsgObject(27,"無效","QRcode(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(28,"呼叫","", MsgObject.mode.n, MsgObject.playsound.call),
                new MsgObject(29,"緊急呼叫","", MsgObject.mode.n, MsgObject.playsound.EmergencyCall),
                new MsgObject(30,"無效","", MsgObject.mode.n, MsgObject.playsound.Authenfail),
                new MsgObject(31,"上班無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(32,"下班無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(33,"上班無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(34,"下班無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(35,"加班上無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(36,"加班下無效","", MsgObject.mode.n, MsgObject.playsound.error),
                new MsgObject(37,"上鎖","", MsgObject.mode.n, MsgObject.playsound.n),
                new MsgObject(38,"求救","一般(讀卡)", MsgObject.mode.uc, MsgObject.playsound.EmergencyCall),
                new MsgObject(39,"電池弱電","一般(讀卡)", MsgObject.mode.uc, MsgObject.playsound.error),
                new MsgObject(40,"eTag讀頭斷線","", MsgObject.mode.n, MsgObject.playsound.disconnected),
                new MsgObject(41,"上班","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(42,"下班","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(43,"加班上","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(44,"加班下","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(45,"加班進","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(46,"加班出","", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(47,"遙控開","一般(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(50,"無效","多門(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(51,"有效","多門(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(52,"樓層無效","樓層管制(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Authenfail),
                new MsgObject(53,"不可通行","樓層管制(讀卡)", MsgObject.mode.sc, MsgObject.playsound.Impassable),
                new MsgObject(55,"樓層有效","樓層管制(讀卡)", MsgObject.mode.uc, MsgObject.playsound.AuthenSuccess),
                new MsgObject(59,"影音讀頭拆除警報","", MsgObject.mode.n, MsgObject.playsound.Violentdamage),
                new MsgObject(60,"讀頭拆除警報","", MsgObject.mode.n, MsgObject.playsound.Violentdamage),
                new MsgObject(61,"呼叫","", MsgObject.mode.n, MsgObject.playsound.call),
                new MsgObject(62,"緊急呼叫","", MsgObject.mode.n, MsgObject.playsound.EmergencyCall),
                }.ToList();
        }

        public class StateObject
        {
            public const int BufferSize = 16;

            public Socket workSocket = null;
            public byte[] _buffer = new byte[BufferSize];
            public byte[] buffer { get { byte[] b = new byte[BufferSize]; Array.Copy(_buffer, b, 16); b[15] = dcuid; return b; } }
            public byte func { get { return _buffer[1]; } }
            public byte year { get { return _buffer[4]; } }
            public byte month { get { return _buffer[5]; } }
            public byte day { get { return _buffer[6]; } }
            public byte hour { get { return _buffer[7]; } }
            public byte minute { get { return _buffer[8]; } }
            public byte second { get { return _buffer[9]; } }
            public DateTime time
            {
                get
                {
                    if (year == 0 & month == 0 & day == 0 & hour == 0 & minute == 0 & second == 0)
                        return DateTime.MinValue;
                    else
                        return DateTime.Parse("20" + year.ToString("00") + "/" + month + "/" + day + " " + hour + ":" + minute + ":" + second);
                }
            }
            public byte dcuid;
            public byte[] ipAddress = new byte[4];
            public byte modid { get { return _buffer[0]; } }
            public byte subfunc { get { return _buffer[2]; } }
            public byte subid { get { return (byte)(_buffer[3] & 0x03); } }
            public bool Status { get { return (_buffer[10] == 0xff); } }

            private int[] UserIDSubfunc = new int[] { 1, 5, 9, 10, 13, 14, 15, 17, 21, 25, 41, 42, 43, 44, 53, 55 };
            public Int16 UserID
            {
                get
                {
                    if (func == 60 && UserIDSubfunc.Contains(subfunc))
                    {
                        return (Int16)(_buffer[10] + (_buffer[11] << 8));
                    }
                    else
                        return 0;
                }
            }

            public string directionString { get { return (direction == 0) ? "進入" : "外出"; } }
            public byte direction { get { return (byte)((_buffer[3] & 0x8) >> 3); } }
            public double CardValue
            {
                get
                {
                    return (double)SiteCode * 65536 + CardCode;
                }
            }

            private int[] SiteCodeSubfunc = new int[] { 4, 5, 8, 10, 16, 31, 32, 35, 36, 37, 38, 45, 46, 47, 52 };
            public UInt16 SiteCode
            {
                get
                {
                    if (func == 60 && SiteCodeSubfunc.Contains(subfunc))
                    {
                        return (UInt16)(_buffer[10] + (_buffer[11] << 8));
                    }
                    else
                        return 0;
                }
            }
            private int[] CardCodeSubfunc = new int[] { 1, 4, 5, 8, 9, 10, 13, 14, 15, 16, 17, 21, 25, 31, 32, 35, 36, 37, 38, 41, 42, 43, 44, 45, 46, 47, 52, 53, 55 };
            public UInt16 CardCode
            {
                get
                {
                    if (func == 60 && CardCodeSubfunc.Contains(subfunc))
                    {
                        return (UInt16)(_buffer[12] + (_buffer[13] << 8));
                    }
                    else
                        return 0;
                }
            }
            private int[] PasswordSubfunc = new int[] { 2, 3, 30, 33, 34, 43, 44 };
            public Int32 Password
            {
                get
                {
                    if (func == 60 && PasswordSubfunc.Contains(subfunc))
                    {
                        return (_buffer[10] + (_buffer[11] << 8) + (_buffer[12] << 16) + (_buffer[13] << 24));
                    }
                    else
                        return 0;
                }
            }
            private string[] rolestrary = new[] { "Unknown", "用戶卡", "管理卡", "巡邏卡", "警衛卡", "清潔卡", "訪客卡", "Unknown" };
            public string roleStr { get { return rolestrary[Role]; } }
            public byte Role { get { return (byte)((_buffer[3] & 0x70) >> 4); } }

            public string objstr(string id, string dev_name, string door_name, string user_name, string group_name)
            {
                if (func == 60)
                {
                    MsgObject mo = MsgConstant.FormatRef.Find(x => x.index == subfunc);
                    string user = "";
                    string info = "";
                    string subinfo = "";
                    switch (mo.InfoMode)
                    {
                        case MsgObject.mode.ppw:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                            subinfo = mo.SubInfo;
                            user = "|";
                            break;
                        case MsgObject.mode.dpw:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                            subinfo = mo.SubInfo;
                            user = "|";
                            break;
                        case MsgObject.mode.sc:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                            subinfo = mo.SubInfo + SiteCode.ToString("00000") + ":" + CardCode.ToString("00000");
                            user = "|";
                            break;
                        case MsgObject.mode.uc:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                            subinfo = mo.SubInfo + " 卡號後五碼:" + CardCode.ToString("00000");
                            user = UserID + "[" + roleStr + "]:" + user_name + "|" + group_name;
                            break;
                        case MsgObject.mode.onoff:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                            subinfo = mo.SubInfo + " " + (Status ? "關閉" : "開啟");
                            user = "|";
                            break;
                        case MsgObject.mode.press:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                            subinfo = mo.SubInfo + " " + (Status ? "放開" : "按下");
                            user = "|";
                            break;
                        case MsgObject.mode.n:
                            info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                            subinfo = mo.SubInfo;
                            user = "|";
                            break;
                        default:
                            break;
                    }
                    return id + "|" + time.ToString() + "|" + dev_name + "|" + door_name + "|" + user + "|" + info + "|" + subinfo;
                }
                return "";
            }


            public class MsgObj
            {
                public DateTime time;
                public string dev_name = "";
                public string door_name = "";
                public string user_name = "";
                public string msg_info = "";
                public string msg_status = "";
            }

            public MsgObj msgObj
            {
                get
                {
                    MsgObj result = new StateObject.MsgObj();

                    if (func == 60)
                    {
                        MsgObject mo = MsgConstant.FormatRef.Find(x => x.index == subfunc);
                        switch (mo.InfoMode)
                        {
                            case MsgObject.mode.ppw:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                result.msg_status = mo.SubInfo;
                                break;
                            case MsgObject.mode.dpw:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                result.msg_status = mo.SubInfo;
                                break;
                            case MsgObject.mode.sc:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                result.msg_status = mo.SubInfo + SiteCode.ToString("00000") + ":" + CardCode.ToString("00000");
                                break;
                            case MsgObject.mode.uc:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                result.msg_status = mo.SubInfo + " 卡號後五碼:" + CardCode.ToString("00000");
                                result.user_name = UserID + "[" + roleStr + "]:";
                                break;
                            case MsgObject.mode.onoff:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                result.msg_status = mo.SubInfo + " " + (Status ? "關閉" : "開啟");
                                break;
                            case MsgObject.mode.press:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                result.msg_status = mo.SubInfo + " " + (Status ? "放開" : "按下");
                                break;
                            case MsgObject.mode.n:
                                result.msg_info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                result.msg_status = mo.SubInfo;
                                break;
                            default:
                                break;
                        }
                    }

                    return result;
                }
            }


            public string msgstr
            {
                get
                {
                    if (func == 60)
                    {
                        MsgObject mo = MsgConstant.FormatRef.Find(x => x.index == subfunc);
                        string user = "";
                        string info = "";
                        string subinfo = "";
                        switch (mo.InfoMode)
                        {
                            case MsgObject.mode.ppw:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                subinfo = mo.SubInfo;
                                user = ",";
                                break;
                            case MsgObject.mode.dpw:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                subinfo = mo.SubInfo;
                                user = ",";
                                break;
                            case MsgObject.mode.sc:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                subinfo = mo.SubInfo + SiteCode.ToString("00000") + ":" + CardCode.ToString("00000");
                                user = ",";
                                break;
                            case MsgObject.mode.uc:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ":" + directionString + ")";
                                subinfo = mo.SubInfo + " 卡號後五碼:" + CardCode.ToString("00000");
                                user = UserID + "[" + roleStr + "]:,GP";
                                break;
                            case MsgObject.mode.onoff:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                subinfo = mo.SubInfo + " " + (Status ? "關閉" : "開啟");
                                user = ",";
                                break;
                            case MsgObject.mode.press:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                subinfo = mo.SubInfo + " " + (Status ? "放開" : "按下");
                                user = ",";
                                break;
                            case MsgObject.mode.n:
                                info = "[" + mo.index + "]" + mo.Info + "(" + subid + ")";
                                subinfo = mo.SubInfo;
                                user = ",";
                                break;
                            default:
                                break;
                        }
                        return "," + time.ToString() + ",,," + user + "," + info + "," + subinfo;
                    }
                    return "";
                }
            }
        }

        public class iACSSocket : IDisposable
        {
            //delegate void CAProcess(ref byte[] msg);
            public bool connected;
            public string CAIpAddress;
            public int Port;
            public Socket Clienter;
            private IPEndPoint ClientIpendpoint;

            #region 初始化以及 連線/斷線
            public iACSSocket(Socket socket, bool already)
            {
                Clienter = socket;
                if (already)
                    connected = true;
                else
                    connected = Connect();
            }
            public iACSSocket(string Ip, int Port = 5889)
            {
                this.CAIpAddress = Ip;
                this.Port = Port;
                ClientIpendpoint = new IPEndPoint(IPAddress.Parse(CAIpAddress), Port);
                Clienter = new Socket(ClientIpendpoint.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                Clienter.ReceiveTimeout = 3000;
                connected = Connect();
            }

            public string SetDCUUploadMsg(bool Enable)//command 36
            {
                byte[] msg = { 8, 0, 0, 36, Convert.ToByte(Enable), 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                if (receivebyte[4] == 32)
                    return "ok";
                else
                    return "error";
            }

            public string SetCAConnection(bool Connect)//command 168
            {
                if (Connect)
                {
                    byte[] msg = { 8, 0, 0, 168, 255, 0, 0, 0, 0, 0 };
                    Clienter.Send(msg, msg.Length, SocketFlags.None);
                    byte[] receivebyte = new byte[32];
                    Clienter.Receive(receivebyte);
                    if (receivebyte[4] == 32)
                        return "ok";
                    else
                        return "error";
                }
                else
                {
                    byte[] msg = { 8, 0, 0, 168, 0, 0, 0, 0, 0, 0 };
                    Clienter.Send(msg, msg.Length, SocketFlags.None);
                    byte[] receivebyte = new byte[32];
                    Clienter.Receive(receivebyte);
                    if (receivebyte[4] == 32)
                        return "ok";
                    else
                        return "error";
                }
            }

            public bool Connect()
            {
                try
                {
                    Clienter.Connect(ClientIpendpoint);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }

            public void Close()
            {
                Clienter.Close();
            }
            #endregion

            public byte[] Send(byte[] msg, int count = 256)
            {
                byte[] receivebytes = new byte[count];

                try
                {
                    int len = Clienter.Send(msg, msg.Length, SocketFlags.None);
                    Clienter.Receive(receivebytes);
                }
                catch (SocketException ex)
                {
                    //connected = false;
                    //cmdSocket = new Socket(remoteServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    //if (Connect(1000))
                    //{
                    //    int len = cmdSocket.Send(msg, msg.Length, SocketFlags.None);
                    //    cmdSocket.Receive(receivebytes);
                    //}
                }

                return receivebytes;

                //if (receivebytes[3] == msg[3] || receivebytes[4] == 32)
                //{
                //}
            }

            #region 時間設定

            public string SetDCUTime(DateTime date) //command 17 
            {
                byte[] msg = { 8, 0, 0, 17, Convert.ToByte(date.Year - 2000), Convert.ToByte(date.Month), Convert.ToByte(date.Day), Convert.ToByte(date.DayOfWeek), Convert.ToByte(date.Hour), Convert.ToByte(date.Minute), Convert.ToByte(date.Second), 0, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                if (receivebyte[4] == 32)
                    return "ok";
                else
                    return "error";
            }

            public DateTime GetDCUTime() //command 18 
            {
                byte[] msg = { 8, 0, 0, 18, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                if (receivebyte[4] != 255)
                    return Convert.ToDateTime((2000 + receivebyte[4]) + "/" + receivebyte[5] + "/" + receivebyte[6] + " " + receivebyte[8] + ":" + receivebyte[9] + ":" + receivebyte[10]);
                else
                    return DateTime.MinValue;
            }

            #endregion

            public string ClearUsers()//command 6
            {
                byte[] msg = new byte[] { 8, 0, 0, 131, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                if (receivebyte[4] == 32)
                    return "ok";
                else
                    return "erroe";
            }

            public int GetDcuUsersCount()//command 40
            {
                byte[] msg = { 8, 0, 0, 40, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                return receivebyte[4] + (receivebyte[5] << 8);
            }

            public string GetDcuUsersData()//command 41 incompleted
            {
                byte[] msg = { 8, 0, 0, 41, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                if (receivebyte[4] != 255)
                    return "ok";
                else
                    return "error";
            }

            public int GetDCUUsers()//command 47
            {
                byte[] msg = { 8, 0, 0, 47, 0, 0, 0, 0, 0, 0 };
                Clienter.Send(msg, msg.Length, SocketFlags.None);
                byte[] receivebyte = new byte[32];
                Clienter.Receive(receivebyte);
                return receivebyte[4] + (receivebyte[5] << 8);
            }

            public void Dispose()
            {
                this.Close();
                GC.Collect();
            }
        }

        public class CommandSocket
        {
            public Socket cmdSocket;
            IPEndPoint remoteServerEP;
            public bool connected = false;
            public int ReceiveTimeout { get { return cmdSocket.ReceiveTimeout; } set { cmdSocket.ReceiveTimeout = value; } }
            public CommandSocket(IPEndPoint remoteServerEP)
            {
                cmdSocket = new Socket(remoteServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                //cmdSocket.ReceiveTimeout = frmCCMSExec.SocketReceiveTimeout;
                this.remoteServerEP = remoteServerEP;
            }

            public bool Connect(int timeout)
            {
                try
                {
                    if (!connected)
                    {
                        DateTime t = DateTime.Now;
                        IAsyncResult result = cmdSocket.BeginConnect(remoteServerEP, null, null);
                        //bool success = result.AsyncWaitHandle.WaitOne(timeout, true);
                        while (!result.IsCompleted)
                        {
                            if (t > t.AddMilliseconds(timeout))
                            {
                                Console.WriteLine("[Failed to connect server.]:" + timeout);
                                // NOTE, MUST CLOSE THE SOCKET
                                cmdSocket.Close();
                                throw new ApplicationException("Failed to connect server.");
                            }
                        }

                        connected = true;

                        try
                        {
                            byte[] receivebytes = new byte[256];
                            var temp = cmdSocket.ReceiveTimeout;
                            cmdSocket.ReceiveTimeout = 1000;
                            cmdSocket.Receive(receivebytes);
                            cmdSocket.ReceiveTimeout = temp;
                        }
                        catch (Exception ex) { }

                    }
                }
                catch (Exception ex)
                {
                    connected = false;
                }
                return connected;
            }

            public bool Connect(bool filter = true)
            {
                try
                {
                    if (!connected)
                    {
                        cmdSocket.Connect(remoteServerEP);
                        connected = true;
                        if (filter)// filter
                            try
                            {
                                byte[] receivebytes = new byte[256];
                                var temp = cmdSocket.ReceiveTimeout;
                                cmdSocket.ReceiveTimeout = 1000;
                                cmdSocket.Receive(receivebytes);
                                cmdSocket.ReceiveTimeout = temp;
                            }
                            catch (Exception) { }
                    }
                }
                catch
                {
                    connected = false;
                }
                return connected;
            }

            public bool Close()
            {
                try
                {
                    cmdSocket.Close();
                    connected = false;
                }
                catch
                {
                }
                return connected;
            }

            public byte[] Send(byte[] msg, int count = 256)
            {
                byte[] receivebytes = new byte[count];

                try
                {
                    int len = cmdSocket.Send(msg, msg.Length, SocketFlags.None);
                    cmdSocket.Receive(receivebytes);
                }
                catch (SocketException ex)
                {
                    Console.WriteLine("[RTimeout]=" + cmdSocket.ReceiveTimeout);
                    Console.WriteLine("[STimeout]=" + cmdSocket.SendTimeout);
                    Console.WriteLine("[Send]" + ex.Message);
                    //connected = false;
                    //cmdSocket = new Socket(remoteServerEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                    //if (Connect(1000))
                    //{
                    //    int len = cmdSocket.Send(msg, msg.Length, SocketFlags.None);
                    //    cmdSocket.Receive(receivebytes);
                    //}
                }

                return receivebytes;

                //if (receivebytes[3] == msg[3] || receivebytes[4] == 32)
                //{
                //}
            }
            public bool CustomSend(byte[] msg)
            {
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }

            public bool DeFire()
            {
                byte[] msg = new byte[10] { 8, 0, 0, 119, 0, 0, 0, 0, 0, 0 };
                //msg[4] = (byte)33;
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }

            public bool DOAction(int modid, int ptno, int action)
            {
                byte[] msg = new byte[10] { 10, 0, 0, 31, 0, 0, 0, 0, 0, 0 };
                msg[2] = (byte)modid;
                msg[5] = (byte)ptno;
                msg[4] = (byte)action;
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }
            public bool DOAction2(int modid, int ptno, int action)
            {
                byte[] msg = new byte[10] { 10, 0, 0, 32, 0, 0, 0, 0, 0, 0 };
                msg[2] = (byte)modid;
                msg[5] = (byte)ptno;
                msg[4] = (byte)action;
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }
            //停止TCP(1689)
            public bool setTCP(bool enable)
            {
                byte[] msg = new byte[10] { 8, 0, 0, 168, ((enable) ? (byte)0xff : (byte)0x00), 0, 0, 0, 0, 0 };
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }

            //暫停回傳
            public bool setUpload(bool enable)
            {
                byte[] msg = new byte[10] { 8, 0, 0, 36, ((enable) ? (byte)1 : (byte)0), 0, 0, 0, 0, 0 };
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }

            //廣播卡機可上傳
            public bool boardcastUpload()
            {
                byte[] msg = new byte[10] { 8, 0, 0, 40, 0, 0, 0, 0, 0, 0 };
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] || receivebytes[4] == 32);
            }

            //polling
            public Tuple<bool, DateTime> polling()
            {
                byte[] msg = { 10, 0, 0, 75, 0, 0, 0, 0, 0, 0 };
                byte[] receivebyte = Send(msg);
                if (receivebyte[3] == 75 && receivebyte[10] == 0xFF && receivebyte[11] == 0xFF)
                {
                    string str = (2000 + receivebyte[4]) + "/" + receivebyte[5] + "/" + receivebyte[6] + " " + receivebyte[7] + ":" + receivebyte[8] + ":" + receivebyte[9];
                    DateTime dt = Convert.ToDateTime(str);
                    return new Tuple<bool, DateTime>(true, dt);// 有回應表5689連線,回應0xFF,0xFF表1689連線
                }
                else if (receivebyte[3] == 75 && receivebyte[10] == 0xFF && receivebyte[11] == 0)
                    return new Tuple<bool, DateTime>(false, DateTime.Now);// 有回應表5689連線,回應0xFF,0x00表1689連線未連線
                else
                    return new Tuple<bool, DateTime>(false, DateTime.Now);// 通訊完全錯誤
            }

            //時間
            public DateTime GetTime() //command 18 
            {
                byte[] msg = { 8, 0, 0, 18, 0, 0, 0, 0, 0, 0 };
                byte[] receivebyte = Send(msg);
                if (receivebyte[4] != 255)
                    return Convert.ToDateTime((2000 + receivebyte[4]) + "/" + receivebyte[5] + "/" + receivebyte[6] + " " + receivebyte[8] + ":" + receivebyte[9] + ":" + receivebyte[10]);
                else
                    return DateTime.MinValue;
            }
            public DateTime GetTime(byte DeviceID) //command 26 
            {
                byte[] msg = { 8, 0, DeviceID, 26, 0, 0, 0, 0, 0, 0 };
                byte[] receivebyte = Send(msg);
                if (receivebyte[4] != 255)
                    return Convert.ToDateTime((2000 + receivebyte[4]) + "/" + receivebyte[5] + "/" + receivebyte[6] + " " + receivebyte[8] + ":" + receivebyte[9] + ":" + receivebyte[10]);
                else
                    return DateTime.MinValue;
            }

            //校正時間
            public bool setTime()
            {
                byte[] msg = new byte[18] { 16, 0, 0, 17, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                DateTime dt = DateTime.Now;
                msg[4] = (byte)(dt.Year - 2000);
                msg[5] = (byte)dt.Month;
                msg[6] = (byte)dt.Day;
                msg[7] = (byte)dt.DayOfWeek;
                msg[8] = (byte)dt.Hour;
                msg[9] = (byte)dt.Minute;
                msg[10] = (byte)dt.Second;
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] && receivebytes[4] == 0x20);
            }
            //校正時間
            public bool setTime(byte devid)
            {
                byte[] msg = new byte[18] { 16, 0, devid, 22, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                DateTime dt = DateTime.Now;
                msg[4] = (byte)(dt.Year - 2000);
                msg[5] = (byte)dt.Month;
                msg[6] = (byte)dt.Day;
                msg[7] = (byte)dt.DayOfWeek;
                msg[8] = (byte)dt.Hour;
                msg[9] = (byte)dt.Minute;
                msg[10] = (byte)dt.Second;
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] && receivebytes[4] == 0x20);
            }

            public void setRapid()
            {
                byte[] msg = new byte[10] { 8, 0, 0, 30, 0, 1, 0, 0, 0, 0 };
                Send(msg);
                Thread.Sleep(2000);
            }

            public byte[] GetIOInfo(byte DeviceID)//command 34 
            {
                byte[] msg = { 8, 0, DeviceID, 34, 0, 0, 0, 0, 0, 0 };

                byte[] receivebyte = Send(msg);
                if (receivebyte[4] != 255 & receivebyte[4] != 255)
                    return new byte[2] { receivebyte[4], receivebyte[5] };
                //St0:b0-按鈕開關(1:按下,0:放開),b2-門位(1:開啟,0:關閉),b3-破壞開關(1:破壞,0:正常)
                //St1: b0 - Alarm Relay(1:Close, 0:Open),b2 - Door Relay(1:Close, 0:Open)
                else
                    return new byte[2];
            }

            //polling設定
            public bool setPolling(bool enable, bool rapid = false)
            {
                byte[] msg;
                msg = new byte[10] { 8, 0, 0, 30, (enable ? (byte)0 : (byte)255), (rapid & enable ? (byte)1 : (byte)0), 0, 0, 0, 0 };
                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] && receivebytes[4] == 0x20);
            }

            //模組連線
            public bool getModConnection(out bool[] moduleConnectedFlag, out int productID, out int firmwareVersion)
            {
                moduleConnectedFlag = new bool[16];
                productID = 0;
                firmwareVersion = 0;
                byte[] msg = { 8, 0, 0, 255, 0, 0, 0, 0, 0, 0 };
                byte[] receivebytes = Send(msg);
                if (receivebytes[3] == msg[3] || receivebytes[4] == 32)
                {
                    productID = receivebytes[4];
                    firmwareVersion = receivebytes[5];
                    for (int k = 0; k < 16; k++)
                        moduleConnectedFlag[k] = Convert.ToBoolean(receivebytes[k + 6]);
                    return true;
                }
                return false;
            }

            //模組連線
            public bool setModParam(byte[] paramBytes)
            {
                byte[] msg = new byte[104];
                msg[0] = 102;
                msg[1] = 0;
                msg[2] = 0;
                msg[3] = 19;

                for (int k = 0; k < 100; k++)
                {
                    msg[4 + k] = paramBytes[k];
                }

                byte[] receivebytes = Send(msg);
                return (receivebytes[3] == msg[3] && receivebytes[4] == 0x20);
            }

            //佇列訊息數量
            public int getMessagesCnt()
            {
                //frmStatusMessage.Show("佇列訊息讀取中...");
                byte[] msg = new byte[10] { 8, 0, 0, 14, 0, 0, 0, 0, 0, 0 };
                byte[] bytes1 = Send(msg);
                int cnt = (int)bytes1[4] + ((int)bytes1[5] << 8);
                return cnt;
            }

            //收佇列訊息
            public List<byte[]> getMessage()
            {
                List<byte[]> result = new List<byte[]>();
                byte[] msg = new byte[10] { 8, 0, 0, 15, 0, 0, 0, 0, 0, 0 };
                int j = 0;
                int cnt = getMessagesCnt();
                while (j < cnt)
                {
                    byte[] bytes2 = Send(msg);
                    if (bytes2[4] == 0xff && bytes2[5] == 0xff)
                        break;
                    else
                    {
                        j++;
                        result.Add(bytes2);
                    }
                    System.Windows.Forms.Application.DoEvents();
                }
                return result;
            }
        }

    }
}
