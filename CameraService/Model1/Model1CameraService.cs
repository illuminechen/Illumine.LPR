using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Illumine.LPR
{
    public class Model1CameraService : ICameraService
    {
        private static Dictionary<IntPtr, int> backFlag = new Dictionary<IntPtr, int>();

        private Model1CameraServiceBase.FNetFindDeviceCallback fNetFindDeviceCallback;

        private static Model1CameraServiceBase.FGetImageCB2 fGetImageCB2;
        private static Action<LPRCameraArgs> Callback;
        private static Action<LPRCameraArgs> BackCallback;
        public void Init()
        {
            Model1CameraServiceBase.Net_Init();
        }

        public void SetCallback(CameraViewerViewModel _, Action<LPRCameraArgs> Callback)
        {
            if (Model1CameraService.Callback == null)
                Model1CameraService.Callback = Callback;
            else
                return;
            if (fGetImageCB2 == null)
                fGetImageCB2 = new Model1CameraServiceBase.FGetImageCB2(FGetImageCB2);
            Model1CameraServiceBase.Net_RegImageRecv2(fGetImageCB2);
        }

        public void SetBackCallback(CameraViewerViewModel _, Action<LPRCameraArgs> Callback)
        {
            if (Model1CameraService.BackCallback == null)
                Model1CameraService.BackCallback = Callback;
        }

        private static int FGetImageCB2(
         int tHandle,
         uint uiImageId,
         ref Model1CameraServiceBase.T_ImageUserInfo2 tImageInfo,
         ref Model1CameraServiceBase.T_PicInfo tPicInfo)
        {
            try
            {
                ChannelViewerViewModel channelViewerViewModel;
                bool isBack = false;
                if (!backFlag.TryGetValue(new IntPtr(tHandle), out int index))
                {
                    channelViewerViewModel = ChannelViewerService.GetList().Find(x =>
                    {
                        if (x == null)
                            return false;
                        IntPtr? playingCameraId = x.CameraViewModel?.PlayingCameraId;
                        IntPtr num = new IntPtr(tHandle);
                        return playingCameraId.GetValueOrDefault() - 1 == num & playingCameraId != null;
                    });

                    if (channelViewerViewModel == null)
                    {
                        LogHelper.Log(nameof(FGetImageCB2), string.Format("CameraViewModel not found @ {0}", (object)tHandle));
                        return -1;
                    }
                }
                else
                {
                    channelViewerViewModel = ChannelViewerService.Get(index);
                    isBack = true;
                }

                Point location = new Point((int)tImageInfo.usLpBox[0], (int)tImageInfo.usLpBox[1]);
                Point point = new Point((int)tImageInfo.usLpBox[2], (int)tImageInfo.usLpBox[3]);
                Size size = new Size(point.X - location.X, point.Y - location.Y);

                if (tPicInfo.ptPanoramaPicBuff == IntPtr.Zero || tPicInfo.uiPanoramaPicLen == 0U)
                {
                    LogHelper.Log("PicInfo incomplete");
                    return 0;
                }
                byte[] numArray = new byte[(int)tPicInfo.uiPanoramaPicLen];
                Marshal.Copy(tPicInfo.ptPanoramaPicBuff, numArray, 0, (int)tPicInfo.uiPanoramaPicLen);

                string str4 = Container.Get<Encoding>().GetString(tImageInfo.szLprResult).Replace("\0", "");
                if (str4 == "拸齪陬")
                    str4 = "無牌車";

                string str5 = str4 == "無牌車" ? Container.Get<LPRSetting>().NoPlate : str4;

                if (isBack)
                    BackCallback(new LPRCameraArgs(str5, new Rectangle(location, size), numArray, channelViewerViewModel.ChannelId));
                else
                    Callback(new LPRCameraArgs(str5, new Rectangle(location, size), numArray, channelViewerViewModel.ChannelId, true));
            }
            catch (Exception ex)
            {
                LogHelper.Log(nameof(FGetImageCB2), ex.ToString());
            }
            return 0;
        }

        public void StartVideo(IntPtr camId, IntPtr handle)
        {
            if (camId == IntPtr.Zero)
                return;

            for (int i = 0; i < 5; i++)
            {
                var ret = Model1CameraServiceBase.Net_StartVideo((int)camId - 1, 0, handle);
                if (ret != -1)
                    return;
                System.Threading.Thread.Sleep(500);
            }
        }

        public void StopVideo(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return;
            for (int i = 0; i < 5; i++)
            {
                var ret = Model1CameraServiceBase.Net_StopVideo((int)camId - 1);
                if (ret != -1)
                    return;
                System.Threading.Thread.Sleep(500);
            }
        }

        public void CheckValid(ref CameraViewerViewModel cvvm)
        {
            if (fNetFindDeviceCallback == null)
                fNetFindDeviceCallback = new Model1CameraServiceBase.FNetFindDeviceCallback(FNetFindDeviceCallback);

            GCHandle handle1 = GCHandle.Alloc(cvvm);
            IntPtr parameter = (IntPtr)handle1;
            // call WinAPi and pass the parameter here
            // then free the handle when not needed:
            // handle1.Free();
            Model1CameraServiceBase.Net_FindDevice(fNetFindDeviceCallback, parameter);
        }

        private int FNetFindDeviceCallback(ref Model1CameraServiceBase.T_RcvMsg ptFindDevice, IntPtr obj)
        {

            // back to object (in callback function):
            GCHandle handle2 = (GCHandle)obj;
            CameraViewerViewModel cvvm = handle2.Target as CameraViewerViewModel;

            byte[] mac = ptFindDevice.tMacSetup.aucMACAddresss;
            string ip = NicHelper.IntToIp(NicHelper.Reverse_uint(ptFindDevice.tNetSetup.uiIPAddress));
            string ip2 = cvvm.ChannelViewModel.Url;
            if (ip != ip2)
                return 0;

            string macAddress = string.Join("-", mac.Select(x => x.ToString("X2")));
            int ptLen = 256; //长度需要大于256
            StringBuilder strEncpyption = new StringBuilder(ptLen);
            int iRet = Model1CameraServiceBase.Net_ReadTwoEncpyption((int)cvvm.camId - 1, strEncpyption, (uint)ptLen);
            if (iRet == -1)
            {
                cvvm.IsValid = false;
                return 0;
            }
            string secret = strEncpyption.ToString();

            cvvm.IsValid = LicenseService.CheckCamera(secret, macAddress);
            LogHelper.Log("IsValid", cvvm.IsValid ? "true" : "false");

            return 0;
        }

        public bool OpenDoor(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return false;

            var config = new Model1CameraServiceBase.T_ControlGate()
            {
                ucState = (byte)1,
                ucReserved = new byte[3]
            };
            return Model1CameraServiceBase.Net_GateSetup((int)camId - 1, ref config) == 0;
        }

        public bool Connect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            int nCamId = Model1CameraServiceBase.Net_AddCamera(cameraViewerViewModel.ChannelViewModel.Url);
            if (nCamId != -1)
            {
                cameraViewerViewModel.camId = (IntPtr)nCamId + 1;
                int iRet = Model1CameraServiceBase.Net_ConnCamera(nCamId, 30000, 10);
                if (iRet != 0)
                {
                    Model1CameraServiceBase.Net_DelCamera(nCamId);
                    return false;
                }
                return true;
            }
            return false;
        }

        public void Disconnect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            if (cameraViewerViewModel.camId != IntPtr.Zero)
            {
                Model1CameraServiceBase.Net_DisConnCamera((int)(cameraViewerViewModel.camId - 1));
                Model1CameraServiceBase.Net_DelCamera((int)(cameraViewerViewModel.camId - 1));
            }
            if (cameraViewerViewModel.backCamId != IntPtr.Zero)
            {
                Model1CameraServiceBase.Net_DisConnCamera((int)(cameraViewerViewModel.backCamId - 1));
                Model1CameraServiceBase.Net_DelCamera((int)(cameraViewerViewModel.backCamId - 1));
            }
        }

        public bool BackConnect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            int nCamId = Model1CameraServiceBase.Net_AddCamera(cameraViewerViewModel.ChannelViewModel.BackUrl);
            if (nCamId != -1)
            {
                cameraViewerViewModel.backCamId = (IntPtr)nCamId + 1;
                int iRet = Model1CameraServiceBase.Net_ConnCamera(nCamId, 30000, 10);
                if (iRet != 0)
                {
                    Model1CameraServiceBase.Net_DelCamera(nCamId);
                    return false;
                }
                return true;
            }
            return false;
        }

        public void BackTrigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;
            if (backFlag.ContainsKey(camId - 1))
                backFlag.Remove(camId - 1);
            backFlag.Add(camId - 1, index);

            Model1CameraServiceBase.T_FrameInfo tFrameInfo = new Model1CameraServiceBase.T_FrameInfo();
            int iRet = Model1CameraServiceBase.Net_ImageSnap((int)camId - 1, ref tFrameInfo);
        }

        public void Trigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;
            if (backFlag.ContainsKey(camId - 1))
                backFlag.Remove(camId - 1);

            Model1CameraServiceBase.T_FrameInfo tFrameInfo = new Model1CameraServiceBase.T_FrameInfo();
            int iRet = Model1CameraServiceBase.Net_ImageSnap((int)camId - 1, ref tFrameInfo);
        }

        public void Send485(IntPtr camId, byte[] data)
        {
            try
            {
                Model1CameraServiceBase.T_RS485Data tRS485Data = new Model1CameraServiceBase.T_RS485Data();

                tRS485Data.data = new byte[1024];
                data.CopyTo(tRS485Data.data, 0);
                tRS485Data.dataLen = (ushort)data.Length;
                tRS485Data.rs485Id = 0;

                Model1CameraServiceBase.Net_SendRS485Data((int)camId - 1, ref tRS485Data);
            }
            catch (Exception ex)
            {
                LogHelper.Log(ex.StackTrace);
                LogHelper.Log(ex.Message);
            }
        }
    }
}
