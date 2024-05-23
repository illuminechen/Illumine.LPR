using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Illumine.LPR
{
    public class DataInitializer
    {
        //private static Model1CameraServiceBase.FGetImageCB2 fGetImageCB2;

        public static void Setup()
        {
            List<(ModbusTCPService, EntryMode)> modbusTcpList = new List<(ModbusTCPService, EntryMode)>();
            Container.Put(RepositoryService.GetMsg());
            Container.Put(RepositoryService.GetVip());
            Container.Put(RepositoryService.GetGroup());
            Container.Put(new RecordPageViewModel());
            Container.Put(new VipListPageViewModel());
            Container.Put(new ConfigPageViewModel());
            Container.Put(new PagingGirdItemsControlViewModel());

            if (Container.Get<LPRSetting>().UseRemoteServer && Container.Get<LPRSetting>().HostIp != "")
            {
                JotangiServerService jServer = new JotangiServerService(Container.Get<LPRSetting>().HostIp, 2048);
                Container.Put<JotangiServerService>(jServer);
                jServer.Start();
            }

            if (Container.Get<LPRSetting>().LPRMode == LPRMode.UDPServer)
            {
                Container.Put(new Dictionary<string, Rectangle>());
                new UDPServer(Container.Get<LPRSetting>().HostIp, Container.Get<LPRSetting>().HostPort).ReceivingCallbak = new EventHandler<LPRArgs>(new Action<object, LPRArgs>(async (sender, e) =>
                {
                    await AuthenticationService.GetLPRAsync(sender, e);
                }));
            }

            foreach (ChannelDataModel channelDataModel in Container.Get<List<ChannelDataModel>>())
            {
                Container.Put(channelDataModel.Id, new Camera());
                var channelVM = new ChannelViewModel()
                {
                    Id = channelDataModel.Id,
                    Url = channelDataModel.Ip,
                    SensorIp = channelDataModel.SensorIp,
                    EtagReaderIp = channelDataModel.EtagReaderIp,
                    BackUrl = (channelDataModel.Ip == channelDataModel.BackCameraIp) ? "" : channelDataModel.BackCameraIp,
                    BackCameraType = channelDataModel.BackCameraType,
                    ExtOutput = channelDataModel.ExtOutput,
                    ExtSubFolder = channelDataModel.ExtSubFolder,
                    Enabled = channelDataModel.Enabled,
                    EntryMode = channelDataModel.EntryMode,
                    PresentMode = channelDataModel.PresentMode,
                    VehicleMode = channelDataModel.VehicleMode,
                    CameraType = channelDataModel.CameraType,
                    InitialTimeout = channelDataModel.InitialTimeout,
                    Led1Ip = channelDataModel.Led1Ip,
                    Led2Ip = channelDataModel.Led2Ip,
                    Led1Port = channelDataModel.Led1Port,
                    Led2Port = channelDataModel.Led2Port,
                    Line1Normal = channelDataModel.Line1Normal,
                    Line1Active = channelDataModel.Line1Active,
                    Line2Active = channelDataModel.Line2Active,
                    Line1NoVip = channelDataModel.Line1NoVip,
                    Line2NoVip = channelDataModel.Line2NoVip,
                    Line2Normal = channelDataModel.Line2Normal,
                    Line1Fail = channelDataModel.Line1Fail,
                    Line2Fail = channelDataModel.Line2Fail,
                    Line1CantPass = channelDataModel.Line1CantPass,
                    Line2CantPass = channelDataModel.Line2CantPass,
                };

                Container.Put(channelDataModel.Id, channelVM);

                if (channelDataModel.Enabled)
                {
                    if (channelDataModel.EntryMode == EntryMode.In)
                    {
                        if (channelDataModel.SensorIp != "")
                        {
                            var modbus = Container.Get<string, ModbusTCPService>(channelDataModel.SensorIp);
                            if (modbus == null)
                            {
                                modbus = new ModbusTCPService(channelDataModel.SensorIp);
                                Container.Put<string, ModbusTCPService>(channelDataModel.SensorIp, modbus);
                            }

                            modbusTcpList.Add((modbus, channelDataModel.EntryMode));
                            modbus.ActuralEnter += (s, e) =>
                            {
                                Container.Get<ChannelViewModel>(channelDataModel.Id).Actual();
                            };
                        }

                        if (channelDataModel.Led1Ip != "")
                        {
                            LEDService.SetWindow(channelDataModel.Led1Ip, channelDataModel.Led1Port, false, channelDataModel.Line2Normal == "");
                            LEDService.Send(channelDataModel.Led1Ip, channelDataModel.Led1Port, channelDataModel.Line1Normal, channelDataModel.Line2Normal, null, null);
                        }
                        if (channelDataModel.Led2Ip != "")
                        {
                            LEDService.SetWindow(channelDataModel.Led2Ip, channelDataModel.Led2Port, true, true);
                            int count = SpaceService.GetSpaceCount();
                            // LEDService.SendStaticText(channelDataModel.Led2Ip, channelDataModel.Led2Port, "剩餘車位");
                            LEDService.SendNumber(channelDataModel.Led2Ip, channelDataModel.Led2Port, count);
                        }
                    }
                    else if (channelDataModel.EntryMode == EntryMode.Out)
                    {
                        if (channelDataModel.SensorIp != "")
                        {
                            var modbus = Container.Get<string, ModbusTCPService>(channelDataModel.SensorIp);
                            if (modbus == null)
                            {
                                modbus = new ModbusTCPService(channelDataModel.SensorIp);
                                Container.Put<string, ModbusTCPService>(channelDataModel.SensorIp, modbus);
                            }
                            modbusTcpList.Add((modbus, channelDataModel.EntryMode));
                            modbus.ActuralLeave += (s, e) =>
                            {
                                Container.Get<ChannelViewModel>(channelDataModel.Id).Actual();
                            };
                        }

                        if (channelDataModel.Led1Ip != "")
                        {
                            LEDService.SetWindow(channelDataModel.Led1Ip, channelDataModel.Led1Port, false, channelDataModel.Line2Normal == "");
                            LEDService.Send(channelDataModel.Led1Ip, channelDataModel.Led1Port, channelDataModel.Line1Normal, channelDataModel.Line2Normal, null, null);
                        }
                    }
                }
                Container.Put(channelDataModel.Id, new ChannelViewerViewModel()
                {
                    ChannelId = channelDataModel.Id,
                    CameraViewModel = new CameraViewerViewModel()
                    {
                        ChannelId = channelDataModel.Id,
                        LPRCallBack = new Action<object, LPRArgs>(async (sender, e) =>
                        {
                            await AuthenticationService.GetLPRAsync(sender, e);
                        })
                    }
                });
                if (channelDataModel.Enabled)
                {
                    Container.Get<ChannelViewerViewModel>(channelDataModel.Id).CameraViewModel.Play();
                    string strImageDirectory = Path.Combine(Container.Get<LPRSetting>().ImageDirectory, string.Format("{0}", channelDataModel.Id));
                    if (string.IsNullOrEmpty(channelDataModel.Watch) && Directory.Exists(channelDataModel.Watch) && Path.GetFullPath(channelDataModel.Watch) != Path.GetFullPath(strImageDirectory))
                    {
                        FileWatcherHelper.RegisterWatcher(channelDataModel.Watch, "*.jpg", (s, e) =>
                       {
                           try
                           {
                               DateTime dateTime = DateTime.Now;
                               dateTime = dateTime.ToLocalTime();
                               string str = Path.Combine(strImageDirectory, (dateTime.ToString("yyyyMMddHHmmssfff") ?? "") + ".jpg");
                               for (int index = 0; index < 10; ++index)
                               {
                                   try
                                   {
                                       File.Copy(e.FullPath, str);
                                   }
                                   catch
                                   {
                                       Thread.Sleep(300);
                                   }
                               }
                               using (UDPClient udpClient = new UDPClient(Container.Get<LPRSetting>().Ip, Container.Get<LPRSetting>().Port))
                                   udpClient.Send(Path.GetFullPath(str));
                           }
                           catch (Exception ex)
                           {
                               LogHelper.Log(nameof(DataInitializer), ex);
                           }
                       });
                        Container.Get<ChannelViewerViewModel>(channelDataModel.Id).CameraViewModel.FileWatching = true;
                    }
                    else
                    {
                        Container.Get<ChannelViewerViewModel>(channelDataModel.Id).CameraViewModel.FileWatching = false;
                        LogHelper.Log(nameof(Setup), channelDataModel.Watch + "is not valid.");
                    }

                    if (Container.Get<LPRSetting>().UseParkingServerEx)
                    {
                        LogHelper.Log("HeartBeat procedure start");
                        Timer timer = new Timer(new TimerCallback(delegate { ParkingServerExService.HeartBeat(TimeHelper.GetEpochTime().ToString(), channelDataModel.Id); }), null, TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(55));
                        Container.Put<int, Timer>(channelDataModel.Id, timer);
                    }
                }
            }
            //List<CameraType> cameraTypes = Container.Get<List<ChannelDataModel>>().Select(o => o.CameraType).ToList();
            //List<CameraType> cameraTypes2 = Container.Get<List<ChannelDataModel>>().Where(o=>o.BackCameraIp!="").Select(o => o.BackCameraType).ToList();

            //cameraTypes.Concat(cameraTypes2).Distinct().ToList().ForEach(x => CameraServiceFactory.Create(x).SetCallback(CameraCallback));
            //if (fGetImageCB2 == null)
            //    fGetImageCB2 = new Model1CameraServiceBase.FGetImageCB2(DataInitializer.FGetImageCB2);
            //Model1CameraServiceBase.Net_RegImageRecv2(fGetImageCB2);

            modbusTcpList.ForEach(x => x.Item1.StartMonitor(x.Item2));
            App.Initialized = true;
        }

        public static void SetupDesignData()
        {
            TypeDescriptor.AddAttributes(typeof(bool[]), new Attribute[1]
            {
                new TypeConverterAttribute(typeof (BoolArrayConverter))
            });
            Container.Put(new RelaySetting
            {
                KeyName = "Com",
                OpenSeconds = 3,
                TriggerRelay = new bool[] { true, true, true, true, true, true, true, true, },
            });
            Container.Put(1, new ChannelViewModel()
            {
                Id = 1,
                Url = "192.168.0.10"
            });
            Container.PutInto(new MsgData()
            {
                Id = 1,
                ChannelId = 1,
                ImagePath = @"D:\code\Illumine.LPR\bin\Debug\Image\1\20210316195243830.jpg",
                PlateNumber = "ABC8888",
                ParkingMode = ParkingMode.SmartPay,
                TimeStamp = DateTime.Now,
                PlateX = 28,
                PlateY = 21,
                Tag = ""
            });
            Container.Put(new List<VipData>()
            {
                new VipData()
                {
                    Id = 1,
                    Name = "老王",
                    Description = "1樓之1",
                    PlateNumber = "TEST5556",
                },
                new VipData()
                {
                    Id = 2,
                    Name = "中王",
                    Description = "1樓之1",
                    PlateNumber = "TEST5557"
                },
                new VipData()
                {
                    Id = 3,
                    Name = "小王",
                    Description = "1樓之1",
                    PlateNumber = "TEST5558"
                }
            });

            Container.Put(new List<GroupData>()
            {
                new GroupData()
                {
                    Id = 1,
                    GroupName = "A",
                    TotalCount = 1,
                    CurrentCount = 0,
                },
                new GroupData()
                {
                    Id = 2,
                    GroupName = "B",
                    TotalCount = 1,
                    CurrentCount = 0,
                },
                new GroupData()
                {
                    Id = 3,
                    GroupName = "C",
                    TotalCount = 1,
                    CurrentCount = 0,
                }
            });

            Container.Put(1, new ChannelViewerViewModel()
            {
                ChannelId = 1,
                RecordViewModel = RecordService.GetViewModel(1),
                CameraViewModel = new CameraViewerViewModel()
                {
                    ChannelId = 1
                }
            });
        }

    }
}
