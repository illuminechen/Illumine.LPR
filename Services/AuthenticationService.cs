using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Illumine.LPR
{

    public static class AuthenticationService
    {
        public static void CameraBackCallback(LPRCameraArgs e)
        {
            e.UseBackCameraToRetry = false;
            CameraCallback(e);
        }

        public static void CameraCallback(LPRCameraArgs e)
        {
            try
            {
                ChannelViewerViewModel channelViewerViewModel = Container.Get<ChannelViewerViewModel>(e.ChannelId);

                if (channelViewerViewModel == null)
                    throw new Exception(string.Format("CameraViewModel not found @ {0}", (object)e.ChannelId));

                CameraViewerViewModel cameraViewModel = channelViewerViewModel.CameraViewModel;

                if (cameraViewModel.FileWatching)
                    throw new Exception("File Watching");

                DateTime localTime = DateTime.Now.ToLocalTime();
                string fileName = localTime.ToString("yyyyMMddHHmmssfff");
                string folderPath = Path.Combine(Container.Get<LPRSetting>().ImageDirectory, string.Format("{0}", (object)cameraViewModel.ChannelId));
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                string fullFileName = Path.Combine(folderPath, fileName + ".jpg");

                e.BigImage.Save(fullFileName, ImageFormat.Jpeg);

                switch (Container.Get<LPRSetting>().LPRMode)
                {
                    case LPRMode.UDPServer:
                        Container.Get<Dictionary<string, Rectangle>>().Put(fullFileName, e.PlateFrame);
                        using (UDPClient udpClient = new UDPClient(Container.Get<LPRSetting>().Ip, Container.Get<LPRSetting>().Port))
                        {
                            udpClient.Send(Path.GetFullPath(fullFileName));
                            break;
                        }
                    case LPRMode.LocalSDK:
                        if (cameraViewModel.LPRCallBack == null)
                            throw new Exception("LPR null");

                        List<PlateDataBundle> plateList = new List<PlateDataBundle>();
                        plateList.Add(new PlateDataBundle()
                        {
                            PlateNumber = e.PlateNumber,
                            Rectangle = e.PlateFrame
                        });

                        LogHelper.Log(string.Format("LPR {0}, {1}, {2}, {3}", fullFileName, localTime, cameraViewModel.ChannelId, plateList));
                        cameraViewModel.LPRCallBack(cameraViewModel, new LPRArgs(fullFileName, localTime, cameraViewModel.ChannelId, plateList, e.UseBackCameraToRetry));
                        break;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log(nameof(CameraCallback), ex.ToString());
            }
        }

        public static async Task GetLPRAsync(object sender, LPRArgs e)
        {
            try
            {
                ChannelViewerViewModel channelViewerViewModel = Container.Get<ChannelViewerViewModel>(e.ChannelId);
                ChannelViewModel channelViewModel = channelViewerViewModel.CameraViewModel.ChannelViewModel;
                PlateDataBundle plateDataBundle = e.PlateList.Count == 0 ? new PlateDataBundle { PlateNumber = Container.Get<LPRSetting>().NoPlate, Rectangle = new Rectangle() } : e.PlateList[0];

                LogHelper.Log(nameof(GetLPRAsync), "retry:" + (e.UseBackCameraToRetry ? "true" : "false") + ",plate:" + plateDataBundle.PlateNumber);

                if (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate)
                {
                    if (e.UseBackCameraToRetry && channelViewModel.BackUrl != "")
                    {
                        CameraServiceFactory.Create(channelViewModel.BackCameraType).BackTrigger(channelViewerViewModel.CameraViewModel.backCamId, e.ChannelId);
                        return;
                    }

                    if (Container.Get<LPRSetting>().FilteringNoPlate)
                        return;
                }
                else if (Container.Get<LPRSetting>().ExtOutput && channelViewModel.ExtOutput)
                {
                    ExtProcess(e.TimeStamp, e.FileName, channelViewModel.ExtSubFolder, channelViewModel.Url, channelViewModel.EntryMode, plateDataBundle.PlateNumber, plateDataBundle.Rectangle, plateDataBundle.EPC, plateDataBundle.TID);
                }



                MsgData msg = new MsgData()
                {
                    Id = -1,
                    ChannelId = e.ChannelId,
                    ImagePath = Path.GetFullPath(e.FileName),
                    PlateX = plateDataBundle.Rectangle.X,
                    PlateY = plateDataBundle.Rectangle.Y,
                    PlateWidth = plateDataBundle.Rectangle.Width,
                    PlateHeight = plateDataBundle.Rectangle.Height,
                    PlateNumber = plateDataBundle.PlateNumber,
                    TimeStamp = e.TimeStamp,
                };

                VipData vip = null;
                if (Container.Get<LPRSetting>().IsVipEnabed)
                {
                    msg.ParkingMode = VipDataService.CheckPlateValid(plateDataBundle.PlateNumber, out vip);
                    if (msg.ParkingMode == ParkingMode.Vip)
                    {
                        plateDataBundle.PlateNumber = vip?.PlateNumber ?? "";
                        msg.PlateNumber = plateDataBundle.PlateNumber;
                    }
                }

                if (Container.Get<LPRSetting>().UsePlateAddingDash)
                {
                    plateDataBundle.PlateNumber = getDashPlate(plateDataBundle.PlateNumber);
                    msg.PlateNumber = plateDataBundle.PlateNumber;
                }

                if (Container.Get<LPRSetting>().IsVipEnabed)
                {
                    DateTime dt = DateTime.Now;
                    var mode = Container.Get<LPRSetting>().ETagMode;

                    switch (mode)
                    {
                        case ETagMode.And:
                            {
                                string eTagNumber = "";
                                while ((DateTime.Now - dt).TotalMilliseconds <= Container.Get<LPRSetting>().ETagWaitingTime)
                                {
                                    if (channelViewModel.EtagNumber != "")
                                    {
                                        LogHelper.Log(eTagNumber);
                                        eTagNumber = channelViewModel.EtagNumber;
                                        channelViewModel.EtagNumber = "";
                                        break;
                                    }
                                }

                                if (eTagNumber == "" || !VipDataService.CheckCoherence(plateDataBundle.PlateNumber, eTagNumber))
                                {
                                    msg.ParkingMode = ParkingMode.NotCoherence;
                                }
                                break;
                            }
                        case ETagMode.Or:
                            {
                                if (msg.ParkingMode == ParkingMode.Vip)
                                    msg.ParkingMode = VipDataService.CheckSpace(channelViewModel.EntryMode, vip);

                                if (msg.ParkingMode != ParkingMode.Vip)
                                {
                                    string eTagNumber = "";
                                    while ((DateTime.Now - dt).TotalMilliseconds <= Container.Get<LPRSetting>().ETagWaitingTime)
                                    {
                                        if (channelViewModel.EtagNumber != "")
                                        {
                                            LogHelper.Log(eTagNumber);
                                            eTagNumber = channelViewModel.EtagNumber;
                                            channelViewModel.EtagNumber = "";
                                            break;
                                        }
                                    }

                                    if (eTagNumber != "")
                                    {
                                        msg.ParkingMode = VipDataService.CheckETagValid(eTagNumber, out var vip2);
                                        if (msg.ParkingMode == ParkingMode.Vip)
                                        {
                                            vip = vip2;
                                        }
                                    }
                                }
                                break;
                            }
                    }
                    if (mode != ETagMode.No)
                        msg.Tag = "eTagNumber=" + vip?.ETagNumber ?? "" + ";";

                    if (msg.ParkingMode == ParkingMode.Vip)
                    {
                        plateDataBundle.PlateNumber = vip?.PlateNumber ?? "";
                        msg.PlateNumber = getDashPlate(plateDataBundle.PlateNumber);
                        msg.ParkingMode = VipDataService.CheckSpace(channelViewModel.EntryMode, vip);
                    }
                }

                PlateSnapshotViewModel snapshotVM = null;

                if (Container.Get<LPRSetting>().IsCheckingInOut && channelViewModel.EntryMode == EntryMode.Out)
                {
                    var lastsnapshot = SnapshotService.GetLastInViewModel(plateDataBundle.PlateNumber);
                    if (lastsnapshot == null)
                    {
                        // 進出不一致
                        msg.ParkingMode = ParkingMode.Incorrect;
                    }
                    else
                    {
                        snapshotVM = lastsnapshot;
                    }
                }
                else
                {
                    snapshotVM = new PlateSnapshotViewModel(msg);
                }

                // 繳費伺服器確認

                if (Container.Get<LPRSetting>().UseParkingServer && channelViewModel.EntryMode == EntryMode.Out)
                {
                    bool pass = await ParkingServerService.CarCheckAsync(plateDataBundle.PlateNumber);

                    if (!pass)
                    {
                        //  show 不可的title (new parking mode) 請至繳費機繳費
                        msg.ParkingMode = ParkingMode.NoPay;
                    }
                }


                channelViewerViewModel.RecordViewModel = new RecordViewModel(msg);
                channelViewerViewModel.PlateSnapshotViewModel = snapshotVM;

                // 先開門

                if (!Container.Get<LPRSetting>().IsVipEnabed || msg.ParkingMode == ParkingMode.Vip)
                {
                    if (channelViewModel.EntryMode == EntryMode.Out)
                    {
                        if (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate)
                        {

                        }
                        else if (!Container.Get<LPRSetting>().UseParkingServer && !Container.Get<LPRSetting>().UseParkingServerEx)
                        {
                            channelViewerViewModel.CameraViewModel.OpenDoor();
                        }

                        if (Container.Get<LPRSetting>().UseParkingServer && msg.ParkingMode != ParkingMode.NoPay)
                        {
                            channelViewerViewModel.CameraViewModel.OpenDoor();
                        }

                        if (Container.Get<LPRSetting>().UseParkingServerEx)
                        {
                            bool pass = await ParkingServerExService.CarPass(channelViewModel.Id, TimeHelper.GetEpochTime().ToString(), plateDataBundle.PlateNumber, Path.GetFullPath(e.FileName));

                            if (!pass)
                            {
                                if (msg.ParkingMode == ParkingMode.Temporary || msg.ParkingMode == ParkingMode.Vip)
                                    msg.ParkingMode = ParkingMode.CantPass;
                            }
                            else
                            {
                                channelViewerViewModel.CameraViewModel.OpenDoor();
                            }
                        }


                        if (channelViewModel.Led1Ip != "")
                        {
                            if (msg.ParkingMode == ParkingMode.CantPass)
                            {
                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, channelViewModel.Line1CantPass, channelViewModel.Line2CantPass, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                            else
                            {
                                string line1 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line1Fail :
                                    (channelViewModel.Line1Active == "[Plate]") ? plateDataBundle.PlateNumber :
                                    (channelViewModel.Line1Active == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line1Active;
                                string line2 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line2Fail :
                                    (channelViewModel.Line2Active == "[Plate]") ? plateDataBundle.PlateNumber :
                                    (channelViewModel.Line2Active == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line2Active;

                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, line1, line2, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                        }

                    }
                    else if (channelViewModel.EntryMode == EntryMode.In)
                    {
                        int count = SpaceService.GetSpaceCount();

                        if (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate)
                        {
                        }
                        else if (count != 0 || channelViewModel.Led2Ip == "") // Led2不計車位 todo:重複進場
                        {
                            if (Container.Get<LPRSetting>().UseParkingServerEx)
                            {
                                bool pass = await ParkingServerExService.CarPass(channelViewModel.Id, TimeHelper.GetEpochTime().ToString(), plateDataBundle.PlateNumber, Path.GetFullPath(e.FileName));

                                if (!pass)
                                {
                                    if (msg.ParkingMode == ParkingMode.Temporary || msg.ParkingMode == ParkingMode.Vip)
                                        msg.ParkingMode = ParkingMode.CantPass;
                                }
                                else
                                {
                                    channelViewerViewModel.CameraViewModel.OpenDoor();
                                }
                            }
                            else
                            {
                                channelViewerViewModel.CameraViewModel.OpenDoor();
                            }
                        }

                        if (channelViewModel.Led1Ip != "")
                        {
                            if (msg.ParkingMode == ParkingMode.CantPass)
                            {
                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, channelViewModel.Line1CantPass, channelViewModel.Line2CantPass, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                            else
                            {
                                string line1 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line1Fail :
                                (channelViewModel.Line1Active == "[Plate]") ? plateDataBundle.PlateNumber :
                                (channelViewModel.Line1Active == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line1Active;
                                string line2 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line2Fail :
                                    (channelViewModel.Line2Active == "[Plate]") ? plateDataBundle.PlateNumber :
                                    (channelViewModel.Line2Active == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line2Active;

                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, line1, line2, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                        }
                    }
                }
                else
                {
                    if (channelViewModel.EntryMode == EntryMode.Out)
                    {
                        if (channelViewModel.Led1Ip != "")
                        {
                            string line1 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line1Fail :
                                (channelViewModel.Line1NoVip == "[Plate]") ? plateDataBundle.PlateNumber :
                                (channelViewModel.Line1NoVip == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line1NoVip;
                            string line2 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line2Fail :
                                (channelViewModel.Line2NoVip == "[Plate]") ? plateDataBundle.PlateNumber :
                                (channelViewModel.Line2NoVip == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line2NoVip;

                            LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, line1, line2, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                        }
                    }
                    else
                    {
                        if (channelViewModel.Led1Ip != "")
                        {
                            if (msg.ParkingMode == ParkingMode.CantPass)
                            {
                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, channelViewModel.Line1CantPass, channelViewModel.Line2CantPass, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                            else
                            {
                                string line1 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line1Fail :
                                (channelViewModel.Line1NoVip == "[Plate]") ? plateDataBundle.PlateNumber :
                                (channelViewModel.Line1NoVip == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line1NoVip;
                                string line2 = (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate) ? channelViewModel.Line2Fail :
                                    (channelViewModel.Line2NoVip == "[Plate]") ? plateDataBundle.PlateNumber :
                                    (channelViewModel.Line2NoVip == "[Vip]") ? VipDataService.GetDisplayText(msg.ParkingMode) : channelViewModel.Line2NoVip;

                                LEDService.Send(channelViewModel.Led1Ip, channelViewModel.Led1Port, line1, line2, channelViewModel.Line1Normal, channelViewModel.Line2Normal);
                            }
                        }
                    }
                    RelayService.OpenRelay(Container.Get<RelaySetting>().PortName, Container.Get<RelaySetting>().TriggerRelay, Container.Get<RelaySetting>().OpenSeconds);
                }


                // 等待確認回應

                if (channelViewModel.SensorIp != "")
                {
                    channelViewModel.Abandon();
                    if (await channelViewModel.WaitActual() == false)
                    {
                        return;
                    }
                }

                // 真正寫入
                int RecordId = Container.PutInto<MsgData>(msg, false);
                channelViewerViewModel.RecordViewModel.Id = RecordId;

                if (channelViewModel.EntryMode == EntryMode.Out)
                {
                    //  場內車位+1
                    int count = SpaceService.GetSpaceCount();
                    string plate = plateDataBundle.PlateNumber;
                    if (msg.ParkingMode == ParkingMode.NoPay)
                    {
                        // 車位不加
                    }
                    else if (msg.ParkingMode == ParkingMode.Temporary || msg.ParkingMode == ParkingMode.Vip)
                    {
                        // 車位要加
                        count++;
                    }
                    if (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate)
                    {
                        plate = "";
                    }

                    // 入口顯示剩餘車位
                    foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                    {
                        if (channelDataModel.Enabled)
                        {
                            if (channelDataModel.EntryMode == EntryMode.In)
                            {
                                if (channelDataModel.Led2Ip != "")
                                {
                                    foreach (var ip in channelDataModel.Led2Ip.Split(';'))
                                    {
                                        if (string.IsNullOrWhiteSpace(ip))
                                            continue;

                                        LEDService.SendNumber(ip, channelDataModel.Led2Port, count);
                                    }
                                }
                            }
                        }
                    }

                    SpaceService.SetSpaceCount(count);
                    if (msg.ParkingMode == ParkingMode.Vip)
                        VipDataService.UpdateGroup(channelViewModel.EntryMode, vip);

                    if (Container.Get<LPRSetting>().UseParkingServer)
                    {
                        //  車出Leave
                        bool pass = await ParkingServerService.CarLeave(plate, e.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), e.FileName);

                        if (!pass)
                            count--;

                        //  count
                        ParkingServerService.SpaceCount(count);

                        //channelViewerViewModel.CameraViewModel.OpenDoor();
                    }

                    if (count == 1)
                    {
                        foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                        {
                            if (channelDataModel.Enabled)
                            {
                                if (channelDataModel.EntryMode == EntryMode.In)
                                {
                                    ChannelViewerViewModel cvvm = Container.Get<ChannelViewerViewModel>(channelDataModel.Id);
                                    ChannelViewModel cvm = cvvm.CameraViewModel.ChannelViewModel;

                                    CameraServiceFactory.Create(cvm.CameraType).Trigger(cvvm.CameraViewModel.camId, cvvm.ChannelId);
                                }
                            }
                        }
                    }
                }
                else if (channelViewModel.EntryMode == EntryMode.In)
                {

                    // 場內車位 -1
                    int count = SpaceService.GetSpaceCount();
                    string plate = plateDataBundle.PlateNumber;
                    if (plateDataBundle.PlateNumber.Replace("-", "") == Container.Get<LPRSetting>().NoPlate)
                    {
                        plate = "";
                    }
                    else
                    {
                        count--;
                        if (count < 0)
                            count = 0;
                        //channelViewerViewModel.CameraViewModel.OpenDoor();
                    }
                    if (Container.Get<LPRSetting>().UseParkingServer)
                    {

                        // 車進 回報
                        ParkingServerService.CarEnter(plate, e.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss"), e.FileName);

                        // count
                        ParkingServerService.SpaceCount(count);

                    }

                    SpaceService.SetSpaceCount(count);
                    if (msg.ParkingMode == ParkingMode.Vip)
                        VipDataService.UpdateGroup(channelViewModel.EntryMode, vip);

                    // 入口顯示剩餘車位
                    foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
                    {
                        if (channelDataModel.Enabled)
                        {
                            if (channelDataModel.EntryMode == EntryMode.In)
                            {
                                if (channelDataModel.Led2Ip != "")
                                {
                                    foreach (var ip in channelDataModel.Led2Ip.Split(';'))
                                    {
                                        if (string.IsNullOrWhiteSpace(ip))
                                            continue;

                                        LEDService.SendNumber(ip, channelDataModel.Led2Port, count);
                                    }
                                }
                            }
                        }
                    }
                }

                channelViewModel.EtagNumber = "";
                msg.Id = RecordId;
                RepositoryService.Insert(msg);
                Container.Get<RecordPageViewModel>().OnPropertyChanged("RecordList");
                Container.Get<RecordPageViewModel>().OnPropertyChanged("RecordCount");
                LogHelper.Log(nameof(GetLPRAsync), e.FileName);
                var c = new RecordViewModelToSnapshotInfoTitleConverter();
                var title = c.Convert(RecordId, null, null, null);
                channelViewerViewModel.CameraViewModel.SendText(title.ToString());
            }
            catch (Exception ex)
            {
                LogHelper.Log(nameof(GetLPRAsync), ex);
            }
        }

        private static string getDashPlate(string plateNumber)
        {
            string number = "0123456789";
            // AAA1234
            // 1234AA
            // 8A1234
            // QM3123 (X
            // A1A321
            // 111AAA
            // A22222

            string text = plateNumber.Replace("-", "");
            bool isNumber = number.Contains(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                var isnum = number.Contains(text[i]);
                if (isnum != isNumber)
                {
                    if (i == 1)
                        i++;
                    if (i == text.Length - 1)
                        i--;
                    plateNumber = text.Substring(0, i) + "-" + text.Substring(i);
                    break;
                }
                isNumber = isnum;
            }

            //if (text.Length > 4)
            //{
            //    if (!number.Contains(ary[0]) || !number.Contains(ary[1]) || !number.Contains(ary[2]))
            //    {
            //        if ((number.Contains(ary[2]) && number.Contains(ary[3])) || (!number.Contains(ary[2]) && !number.Contains(ary[3])))
            //        {
            //            plateNumber = text.Substring(0, 2) + "-" + text.Substring(2, len - 2);
            //        }
            //        else
            //        {
            //            plateNumber = text.Substring(0, 3) + "-" + text.Substring(3, len - 3);
            //        }
            //    }
            //    else
            //    {
            //        if ((number.Contains(ary[2]) && number.Contains(ary[3])) || (!number.Contains(ary[2]) && !number.Contains(ary[3])))
            //        {
            //            plateNumber = text.Substring(0, len - 2) + "-" + text.Substring(len - 2, 2);
            //        }
            //        else
            //        {
            //            plateNumber = text.Substring(0, 3) + "-" + text.Substring(3, len - 3);
            //        }
            //    }
            //}
            return plateNumber;
        }

        public static void ExtProcess(
             DateTime timeStamp,
             string filename,
             string subfolder,
             string ip,
             EntryMode entryMode,
             string plateNumber,
             Rectangle rectangle,
             string epc,
             string tid)
        {
            string extFolder = Container.Get<LPRSetting>().ExtFolder;
            if (!Directory.Exists(extFolder))
                Directory.CreateDirectory(extFolder);
            if (!string.IsNullOrWhiteSpace(subfolder))
            {
                extFolder = Path.Combine(extFolder, subfolder);
                if (!Directory.Exists(Path.Combine(extFolder, subfolder)))
                    Directory.CreateDirectory(extFolder);
            }
            string str1 = timeStamp.ToString("yyyyMMddHHmmssfff");
            string str2 = string.Join("", ((IEnumerable<string>)ip.Split('.')).Select<string, string>((Func<string, string>)(x => int.Parse(x).ToString("000"))));
            string str3 = Path.Combine(extFolder, string.Format("{0}-PSS-{1}_{2}_{3}", str2, (int)entryMode, str1, plateNumber));
            using (StreamWriter streamWriter = new StreamWriter(str3 + ".txt", false, Container.Get<Encoding>()))
                streamWriter.WriteLine(string.Format("epc=,tid=,vip=0,LPR={0},COORD={1},{2},{3},{4}", plateNumber, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
            LogHelper.Log("ext", string.Format("epc=,tid=,vip=0,LPR={0},COORD={1},{2},{3},{4}", plateNumber, rectangle.Left, rectangle.Top, rectangle.Right, rectangle.Bottom));
            File.Copy(filename, str3 + ".jpg");
            LogHelper.Log("ext", str3 + ".jpg");

        }

    }
}
