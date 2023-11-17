using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class LEDService
    {
        public enum CommandType { Text, Date, Time, SetWindow, Number };
        public class TextSendingQueue
        {
            public CommandType Command { get; set; }
            public string Text { get; set; }

            public int Line { get; set; }

            public int Width { get; set; }
            public int Height { get; set; }

            public DateTime ActTime { get; set; } = DateTime.MinValue;

            public bool Full { get; set; }
            public bool Single { get; set; }

            public string Ip { get; set; }
            public int Port { get; set; }
        }
        static Dictionary<Tuple<string, int>, Queue<TextSendingQueue>> QueueDict = new Dictionary<Tuple<string, int>, Queue<TextSendingQueue>>();
        static Dictionary<Tuple<string, int>, Dictionary<int, DateTime>> SuppressDict = new Dictionary<Tuple<string, int>, Dictionary<int, DateTime>>();
        static bool Starting = false;
        public static void StopServer()
        {
            Starting = false;
        }

        public static void StartServer()
        {
            Starting = true;
            int interval = 250;
            Task.Run(async () =>
            {
                while (Starting)
                {
                    if (QueueDict.Count == 0)
                        await Task.Delay(interval);
                    int _interval = interval;
                    foreach (var kv in QueueDict)
                    {
                        int nRet = 0;

                        var SendingQueue = new Queue<TextSendingQueue>(kv.Value);
                        kv.Value.Clear();

                        nRet = InitComm(kv.Key.Item1, kv.Key.Item2);
                        await Task.Delay(200);
                        _interval -= 200;

                        while (SendingQueue.Count > 0)
                        {
                            var obj = SendingQueue.Dequeue();
                            if (obj.ActTime > DateTime.Now)
                            {
                                QueueDict[kv.Key].Enqueue(obj);
                                continue;
                            }

                            string Ip = obj.Ip;
                            int Port = obj.Port;
                            int line = obj.Line;
                            string SendingText = obj.Text;

                            try
                            {
                                switch (obj.Command)
                                {
                                    case CommandType.Text:
                                        nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(SendingText), 0xFF, 16, 3, 0, 3, 1);
                                        break;
                                    //case CommandType.Wait:
                                    //    await Task.Delay(obj.WaitTime);
                                    //    break;
                                    case CommandType.Date:
                                        nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(DateTime.Today.ToString("yyyy-MM-dd")), 0xFF, 8, 3, 0, 3, 1);
                                        obj.ActTime = DateTime.Now + TimeSpan.FromSeconds(1);
                                        if (!SuppressDict.ContainsKey(kv.Key) || !SuppressDict[kv.Key].ContainsKey(line) || SuppressDict[kv.Key][line] < DateTime.Now)
                                            QueueDict[kv.Key].Enqueue(obj);
                                        break;
                                    case CommandType.Time:
                                        //byte[] byTimeInfo = new byte[7];
                                        DateTime curTime;
                                        curTime = DateTime.Now;
                                        //byTimeInfo[0] = Convert.ToByte(curTime.Second);
                                        //byTimeInfo[1] = Convert.ToByte(curTime.Minute);
                                        //byTimeInfo[2] = Convert.ToByte(curTime.Hour);
                                        //byTimeInfo[3] = Convert.ToByte(curTime.DayOfWeek);
                                        //byTimeInfo[4] = Convert.ToByte(curTime.Day);
                                        //byTimeInfo[5] = Convert.ToByte(curTime.Month);
                                        //byTimeInfo[6] = Convert.ToByte(curTime.Year - 2000);
                                        //nRet = CP5200.CP5200_Net_SetTime(Convert.ToByte(1), byTimeInfo);
                                        string timestr = curTime.Hour.ToString("00") + ":" + curTime.Minute.ToString("00") + ":" + curTime.Second.ToString("00");
                                        nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(timestr), 0xFF, 16, 3, 0, 3, 1);
                                        obj.ActTime = DateTime.Now + TimeSpan.FromSeconds(0.5);
                                        if (!SuppressDict.ContainsKey(kv.Key) || !SuppressDict[kv.Key].ContainsKey(line) || SuppressDict[kv.Key][line] < DateTime.Now)
                                            QueueDict[kv.Key].Enqueue(obj);
                                        break;
                                    case CommandType.SetWindow:
                                        int[] nWndRect = new int[4];
                                        if (!obj.Single)
                                            nWndRect = new int[8];
                                        GetSplitWnd(obj.Width, obj.Height, nWndRect, obj.Full);
                                        int ret = CP5200.CP5200_Net_SplitScreen(Convert.ToByte(1), obj.Width, obj.Height, obj.Single ? 1 : 2, nWndRect);
                                        break;
                                    case CommandType.Number:
                                        nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), 0, Marshal.StringToHGlobalAnsi(SendingText), 0xFF, 32, 3, 0, 3, 1);
                                        break;
                                }
                                if (nRet < 0)
                                    LogHelper.Log(obj.Command.ToString() + " Failed:" + Ip + ":" + Port);
                            }
                            catch (Exception ex)
                            {
                                LogHelper.Log(ex.Message);
                            }
                        }
                        if (_interval > 0)
                            await Task.Delay(_interval);
                    }
                    //var SendingQueue = new Queue<TextSendingQueue>(BufferQueue.ToList().OrderBy(x => x.Ip));
                    //BufferQueue.Clear();
                    //foreach (var o in SendingQueue)
                    //{
                    //    o.Register--;
                    //}
                    //if (_interval > 0)
                    //    await Task.Delay(_interval);
                    //while (SendingQueue.Count > 0)
                    //{
                    //    var obj = SendingQueue.Dequeue();

                    //    if (obj.Register > 0)
                    //    {
                    //        BufferQueue.Enqueue(obj);
                    //        continue;
                    //    }

                    //    string Ip = obj.Ip;
                    //    int Port = obj.Port;
                    //    int line = obj.Line;
                    //    string SendingText = obj.Text;
                    //    int nRet = 0;

                    //    LogHelper.Log("Ip :" + Ip + "Port:" + Port + ",line:" + line + ",SendingText:" + SendingText);
                    //    if (lastobj != null && lastobj.Ip == obj.Ip && lastobj.Port == obj.Port)
                    //    {

                    //    }
                    //    else
                    //    {
                    //        nRet = InitComm(obj.Ip, obj.Port);
                    //        await Task.Delay(200);
                    //        _interval -= 200;
                    //    }
                    //    try
                    //    {
                    //        switch (obj.Command)
                    //        {
                    //            case CommandType.Text:
                    //                nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(SendingText), 0xFF, 16, 3, 0, 3, 1);
                    //                break;
                    //            //case CommandType.Wait:
                    //            //    await Task.Delay(obj.WaitTime);
                    //            //    break;
                    //            case CommandType.Date:
                    //                nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(DateTime.Today.ToString("yyyy年MM月dd日")), 0xFF, 16, 3, 0, 3, 1);
                    //                obj.Register = 1;
                    //                BufferQueue.Enqueue(obj);
                    //                break;
                    //            case CommandType.Time:
                    //                //byte[] byTimeInfo = new byte[7];
                    //                DateTime curTime;
                    //                curTime = DateTime.Now;
                    //                //byTimeInfo[0] = Convert.ToByte(curTime.Second);
                    //                //byTimeInfo[1] = Convert.ToByte(curTime.Minute);
                    //                //byTimeInfo[2] = Convert.ToByte(curTime.Hour);
                    //                //byTimeInfo[3] = Convert.ToByte(curTime.DayOfWeek);
                    //                //byTimeInfo[4] = Convert.ToByte(curTime.Day);
                    //                //byTimeInfo[5] = Convert.ToByte(curTime.Month);
                    //                //byTimeInfo[6] = Convert.ToByte(curTime.Year - 2000);
                    //                //nRet = CP5200.CP5200_Net_SetTime(Convert.ToByte(1), byTimeInfo);
                    //                string timestr = curTime.Hour.ToString("00") + ":" + curTime.Minute.ToString("00") + ":" + curTime.Second.ToString("00");
                    //                nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), line, Marshal.StringToHGlobalAnsi(timestr), 0xFF, 16, 3, 0, 3, 1);
                    //                obj.Register = 1;
                    //                BufferQueue.Enqueue(obj);
                    //                break;
                    //            case CommandType.SetWindow:
                    //                int[] nWndRect = new int[4];
                    //                if (!obj.Single)
                    //                    nWndRect = new int[8];
                    //                GetSplitWnd(obj.Width, obj.Height, nWndRect, obj.Full);
                    //                int ret = CP5200.CP5200_Net_SplitScreen(Convert.ToByte(1), obj.Width, obj.Height, obj.Single ? 1 : 2, nWndRect);
                    //                break;
                    //            case CommandType.Number:
                    //                nRet = CP5200.CP5200_Net_SendText(Convert.ToByte(1), 0, Marshal.StringToHGlobalAnsi(SendingText), 0xFF, 32, 3, 0, 3, 1);
                    //                break;
                    //        }
                    //        if (nRet < 0)
                    //            LogHelper.Log(obj.Command.ToString() + " Failed:" + Ip + ":" + Port);
                    //    }
                    //    catch (Exception ex)
                    //    {
                    //        LogHelper.Log(ex.Message);
                    //    }
                    //}
                }
            });
        }

        private static uint GetIP(string strIp)
        {
            System.Net.IPAddress ipaddress = System.Net.IPAddress.Parse(strIp);
            uint lIp = (uint)ipaddress.Address;
            lIp = ((lIp & 0xFF000000) >> 24) + ((lIp & 0x00FF0000) >> 8) + ((lIp & 0x0000FF00) << 8) + ((lIp & 0x000000FF) << 24);
            return (lIp);
        }

        private static int InitComm(string ip, int port)
        {
            int ret = 0;
            uint dwIPAddr = GetIP(ip);
            uint dwIDCode = GetIP("255.255.255.255");
            int nIPPort = Convert.ToInt32(port);
            if (dwIPAddr != 0 && dwIDCode != 0)
                ret = CP5200.CP5200_Net_Init(dwIPAddr, nIPPort, dwIDCode, 600);
            return ret;
        }

        private static void GetSplitWnd(int width, int height, int[] rcWins, bool full)
        {
            int nWidth = Convert.ToInt32(width);
            int nHeight = Convert.ToInt32(height);
            if (rcWins.Length == 4)
            {
                rcWins[0] = 0;
                rcWins[1] = full ? 0 : nHeight / 4;
                rcWins[2] = nWidth;
                rcWins[3] = full ? nHeight : 3 * nHeight / 4;
            }
            else
            {
                rcWins[0] = 0;
                rcWins[1] = 0;
                rcWins[2] = nWidth;
                rcWins[3] = nHeight / 2;
                rcWins[4] = 0;
                rcWins[5] = nHeight / 2;
                rcWins[6] = nWidth;
                rcWins[7] = nHeight;
            }
            //rcWins[4] = 0;
            //rcWins[5] = nHeight / 2;
            //rcWins[6] = nWidth;
            //rcWins[7] = nHeight;
        }
        public static void SetWindow(string ip, int port, bool full, bool single, int width = 64, int height = 32)
        {
            LogHelper.Log("SetWindow :" + ip + ":" + port + ">" + width + "," + height + ",full:" + (full ? "true" : "false") + ", single:" + (single ? "true" : "false"));
            if (!QueueDict.ContainsKey(new Tuple<string, int>(ip, port)))
            {
                QueueDict.Add(new Tuple<string, int>(ip, port), new Queue<TextSendingQueue>());
            }
            var queue = QueueDict[new Tuple<string, int>(ip, port)];

            queue.Enqueue(new TextSendingQueue() { Ip = ip, Port = port, Full = full, Single = single, Width = width, Height = height, Command = CommandType.SetWindow });
        }

        public static void SendNumber(string ip, int port, int num)
        {
            LogHelper.Log("SendText :" + ip + ":" + port + ">" + num);
            if (!QueueDict.ContainsKey(new Tuple<string, int>(ip, port)))
            {
                QueueDict.Add(new Tuple<string, int>(ip, port), new Queue<TextSendingQueue>());
            }
            var queue = QueueDict[new Tuple<string, int>(ip, port)];

            queue.Enqueue(new TextSendingQueue() { Ip = ip, Port = port, Text = num.ToString("000"), Command = CommandType.Number });
        }

        //public static TextSendingQueue SendText(string text)
        //{
        //    return new TextSendingQueue() { Text = text, Command = 1 };
        //    //if (!string.IsNullOrWhiteSpace(staticText))
        //    //{
        //    //    obj.WaitTime = 8000;
        //    //}
        //    //SendingQueue.Enqueue(obj);
        //    //if (!string.IsNullOrWhiteSpace(staticText))
        //    //    SendingQueue.Enqueue(new TextSendingQueue() { Ip = ip, Port = port, Text = staticText, Command = 1, Line = line });

        //    //return { new TextSendingQueue() { Ip = ip, Port = port, Text = num.ToString("000"), Command = 2 } };

        //}

        //public static TextSendingQueue SendStaticText(string text)
        //{
        //    return new TextSendingQueue() { Text = text, Command = 1 };
        //}

        //private static TextSendingQueue SetTime()
        //{
        //    return new TextSendingQueue() { Command = 4 };
        //}

        //public static TextSendingQueue SendTime()
        //{
        //    return new TextSendingQueue() { Command = 5 };
        //}

        //public static TextSendingQueue SendDate()
        //{
        //    return new TextSendingQueue() { Command = 6 };
        //}


        public static void Send(string ip, int port, string active1, string active2, string normal1, string normal2)
        {

            TextSendingQueue obj1 = new TextSendingQueue() { Ip = ip, Port = port, Line = 0, ActTime = DateTime.Now };
            TextSendingQueue obj2 = new TextSendingQueue() { Ip = ip, Port = port, Line = 1, ActTime = DateTime.Now };
            var key = new Tuple<string, int>(ip, port);
            if (!QueueDict.ContainsKey(key))
                QueueDict.Add(key, new Queue<TextSendingQueue>());
            var queue = QueueDict[key];
            var temp = new Queue<TextSendingQueue>();
            foreach (var obj in queue)
            {
                if (obj.Command == CommandType.SetWindow)
                    temp.Enqueue(obj);
            }
            queue.Clear();
            foreach (var obj in temp)
                queue.Enqueue(obj);

            //BufferQueue = new Queue<TextSendingQueue>(BufferQueue.ToList().Where(x => x.Ip != ip && x.Port != port));
            switch (active1)
            {
                case "[Date]":
                    obj1.Command = CommandType.Date;

                    queue.Enqueue(obj1);
                    break;
                case "[Time]":
                    obj1.Command = CommandType.Time;
                    queue.Enqueue(obj1);
                    break;
                default:
                    if (!string.IsNullOrEmpty(active1))
                    {
                        if (!SuppressDict.ContainsKey(key))
                            SuppressDict.Add(key, new Dictionary<int, DateTime>() { { 0, DateTime.Now + TimeSpan.FromSeconds(8) } });
                        else if (!SuppressDict[key].ContainsKey(0))
                            SuppressDict[key].Add(0, DateTime.Now + TimeSpan.FromSeconds(8));
                        else
                            SuppressDict[key][0] = DateTime.Now + TimeSpan.FromSeconds(8);

                        obj1.Command = CommandType.Text;
                        obj1.Text = active1;
                        queue.Enqueue(obj1);
                    }
                    break;
            }

            switch (active2)
            {
                case "[Date]":
                    obj2.Command = CommandType.Date;
                    queue.Enqueue(obj2);
                    break;
                case "[Time]":
                    obj2.Command = CommandType.Time;
                    queue.Enqueue(obj2);
                    break;
                default:
                    if (!string.IsNullOrEmpty(active2))
                    {
                        if (!SuppressDict.ContainsKey(key))
                            SuppressDict.Add(key, new Dictionary<int, DateTime>() { { 1, DateTime.Now + TimeSpan.FromSeconds(8) } });
                        else if (!SuppressDict[key].ContainsKey(1))
                            SuppressDict[key].Add(1, DateTime.Now + TimeSpan.FromSeconds(8));
                        else
                            SuppressDict[key][1] = DateTime.Now + TimeSpan.FromSeconds(8);

                        obj2.Command = CommandType.Text;
                        obj2.Text = active2;
                        queue.Enqueue(obj2);
                    }
                    break;
            }

            obj1 = new TextSendingQueue() { Ip = ip, Port = port, Line = 0, ActTime = DateTime.Now + TimeSpan.FromSeconds(8) };
            obj2 = new TextSendingQueue() { Ip = ip, Port = port, Line = 1, ActTime = DateTime.Now + TimeSpan.FromSeconds(8) };

            switch (normal1)
            {
                case "[Date]":
                    obj1.Command = CommandType.Date;
                    queue.Enqueue(obj1);
                    break;
                case "[Time]":
                    obj1.Command = CommandType.Time;
                    queue.Enqueue(obj1);
                    break;
                default:
                    if (!string.IsNullOrEmpty(normal1))
                    {
                        obj1.Command = CommandType.Text;
                        obj1.Text = normal1;
                        queue.Enqueue(obj1);
                    }
                    break;
            }
            switch (normal2)
            {
                case "[Date]":
                    obj2.Command = CommandType.Date;
                    queue.Enqueue(obj2);
                    break;
                case "[Time]":
                    obj2.Command = CommandType.Time;
                    queue.Enqueue(obj2);
                    break;
                default:
                    if (!string.IsNullOrEmpty(normal2))
                    {
                        obj2.Command = CommandType.Text;
                        obj2.Text = normal2;
                        queue.Enqueue(obj2);
                    }
                    break;
            }
        }

        //public static void SendStaticText(string ip, int port, string text, int width = 64, int height = 32)
        //{
        //    LogHelper.Log("SendStaticText :" + ip + ":" + port + ">" + text);
        //    Task.Run(() =>
        //    {
        //        int nRet = 0;
        //        try
        //        {
        //            nRet = InitComm(ip, port);
        //            nRet = SetWindow(ip, port);
        //            int[] nWndRect = new int[8];
        //            GetSplitWnd(width, height, nWndRect);
        //            CP5200.CP5200_Net_SendStatic(Convert.ToByte(1), 0, Marshal.StringToHGlobalAnsi(text), 0xFF, 16, 0, 0, 0, nWndRect[2] - nWndRect[0], nWndRect[3] - nWndRect[1]);
        //            if (nRet < 0)
        //                LogHelper.Log("SendStaticText Failed:" + ip + ":" + port + ">" + text);
        //        }
        //        catch (Exception ex)
        //        {
        //            LogHelper.Log(ex.Message);
        //        }
        //        return (nRet >= 0);
        //    });
        //}
    }
}
