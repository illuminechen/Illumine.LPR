
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Illumine.LPR
{
    public class ICE_engCameraService : ICameraService
    {
        private static Dictionary<IntPtr, int> backFlag = new Dictionary<IntPtr, int>();

        private static ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate onPlate;
        private static ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate backSDK_onPlate;

        private static Action<LPRCameraArgs> Callback;
        private static Action<LPRCameraArgs> BackCallback;

        public void CheckValid(ref CameraViewerViewModel cvvm)
        {
            StringBuilder strResult = new StringBuilder(4096);
            ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_GetDevID(cvvm.camId, strResult);//获取相机mac地址
            string rawMacAddress = strResult.ToString();

            string macAddress = string.Join("-", Enumerable.Range(0, rawMacAddress.Length / 2)
            .Select(i => rawMacAddress.Substring(i * 2, Math.Min(rawMacAddress.Length - i * 2, 2))));

            byte[] recvData = new byte[256];
            ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_ReadUserData(cvvm.camId, recvData, 256);//读取数据

            string secret = Container.Get<Encoding>().GetString(recvData).Trim('\0');

            cvvm.IsValid = LicenseService.CheckCamera(secret, macAddress);
        }

        public bool Connect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            IntPtr ret = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OpenDevice(cameraViewerViewModel.ChannelViewModel.Url);
            if (IntPtr.Zero == ret)
            {
                return false;
            }

            cameraViewerViewModel.camId = ret;
            return true;
        }
        public void Disconnect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            if (cameraViewerViewModel.camId != IntPtr.Zero)
                ICECameraServiceBase.ipcsdk.ICE_IPCSDK_Close(cameraViewerViewModel.camId);

            cameraViewerViewModel.camId = IntPtr.Zero;

            if (cameraViewerViewModel.backCamId != IntPtr.Zero)
                ICECameraServiceBase.ipcsdk.ICE_IPCSDK_Close(cameraViewerViewModel.backCamId);

            cameraViewerViewModel.backCamId = IntPtr.Zero;
        }

        public bool BackConnect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            IntPtr ret = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OpenDevice(cameraViewerViewModel.ChannelViewModel.BackUrl);
            if (IntPtr.Zero == ret)
            {
                return false;
            }

            cameraViewerViewModel.backCamId = ret;

            return true;
        }

        public void Init()
        {
            ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_Init();
        }

        public bool OpenDoor(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return false;

            uint nRet = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OpenGate(camId);
            return nRet == 1;
        }

        public void SetCallback(CameraViewerViewModel cvvm, Action<LPRCameraArgs> Callback)
        {
            if (ICE_engCameraService.Callback == null)
                ICE_engCameraService.Callback = Callback;

            if (onPlate == null)
                onPlate = new ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate(SDK_OnPlate);

            if (cvvm.ChannelViewModel.Enabled)
            {
                if (cvvm.ChannelViewModel.CameraType == CameraType.Ice_eng)
                {
                    ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_SetPlateCallback(cvvm.camId, onPlate, new IntPtr(cvvm.ChannelId));
                    for (int i = 0; i < 5; i++)
                    {
                        uint ret = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_RegLprResult(cvvm.camId, 1, 0);
                        if (ret == 1)
                        {
                            LogHelper.Log("ICE_eng:SetCallback" + i, string.Format("Success:{0}-{1}", (object)cvvm.ChannelId, cvvm.camId));
                            break;
                        }
                    }
                }
            }
        }

        public void SetBackCallback(CameraViewerViewModel cvvm, Action<LPRCameraArgs> Callback)
        {
            if (ICE_engCameraService.BackCallback == null)
                ICE_engCameraService.BackCallback = Callback;

            if (backSDK_onPlate == null)
                backSDK_onPlate = new ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate(BackSDK_OnPlate);

            if (cvvm.ChannelViewModel.Enabled)
            {
                if (cvvm.ChannelViewModel.BackCameraType == CameraType.Ice_eng)
                {
                    ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_SetPlateCallback(cvvm.backCamId, backSDK_onPlate, cvvm.backCamId);
                    for (int i = 0; i < 5; i++)
                    {
                        uint ret = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_RegLprResult(cvvm.backCamId, 1, 0);
                        if (ret == 1)
                        {
                            LogHelper.Log("ICE_eng:SetCallback", string.Format("Success:back{0}-{1}", (object)cvvm.ChannelId, cvvm.backCamId));
                            break;
                        }
                    }
                }
            }
        }

        private void BackSDK_OnPlate(System.IntPtr pvParam,
   [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP,
   ref ICE_engCameraServiceBase.T_LprResult tLprResult, System.IntPtr pFullPicData, int fullPicLen, System.IntPtr pPlatePicData, int platePiclen, uint u32Reserved1, uint u32Reserved2)
        {
            if (!backFlag.TryGetValue(pvParam, out int index))
            {
                return;
            }
            backFlag.Remove(pvParam);

            byte[] buff = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, tLprResult.plateInfo.plateNum);
            string pcNumber = Encoding.Unicode.GetString(buff);
            pcNumber = pcNumber.Substring(0, pcNumber.IndexOf('\0'));
            pcNumber = pcNumber == "" ? Container.Get<LPRSetting>().NoPlate : pcNumber;

            byte[] datajpg2 = null;
            if (fullPicLen > 0)
            {
                IntPtr ptr2 = (IntPtr)pFullPicData;
                datajpg2 = new byte[fullPicLen];
                Marshal.Copy(ptr2, datajpg2, 0, datajpg2.Length);
            }

            int left = tLprResult.plateInfo.stPlateRect.s16Left;
            int right = tLprResult.plateInfo.stPlateRect.s16Right;
            int top = tLprResult.plateInfo.stPlateRect.s16Top;
            int bottom = tLprResult.plateInfo.stPlateRect.s16Bottom;

            BackCallback(new LPRCameraArgs(pcNumber, new Rectangle(left, top, Math.Abs(right - left), Math.Abs(bottom - top)), datajpg2, index));
        }



        private void SDK_OnPlate(System.IntPtr pvParam,
           [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP,
           ref ICE_engCameraServiceBase.T_LprResult tLprResult, System.IntPtr pFullPicData, int fullPicLen, System.IntPtr pPlatePicData, int platePiclen, uint u32Reserved1, uint u32Reserved2)
        {
            int index = (int)pvParam;

            byte[] buff = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, tLprResult.plateInfo.plateNum);
            string pcNumber = Encoding.Unicode.GetString(buff);
            pcNumber = pcNumber.Substring(0, pcNumber.IndexOf('\0'));
            pcNumber = pcNumber == "" ? Container.Get<LPRSetting>().NoPlate : pcNumber;

            byte[] datajpg2 = null;
            if (fullPicLen > 0)
            {
                IntPtr ptr2 = (IntPtr)pFullPicData;
                datajpg2 = new byte[fullPicLen];
                Marshal.Copy(ptr2, datajpg2, 0, datajpg2.Length);
            }

            int left = tLprResult.plateInfo.stPlateRect.s16Left;
            int right = tLprResult.plateInfo.stPlateRect.s16Right;
            int top = tLprResult.plateInfo.stPlateRect.s16Top;
            int bottom = tLprResult.plateInfo.stPlateRect.s16Bottom;

            if (pcNumber == "拸齪陬")
                pcNumber = "";

            pcNumber = pcNumber == "" ? Container.Get<LPRSetting>().NoPlate : pcNumber;

            Callback(new LPRCameraArgs(pcNumber, new Rectangle(left, top, Math.Abs(right - left), Math.Abs(bottom - top)), datajpg2, index, true));
        }

        public void StartVideo(IntPtr camId, IntPtr handle)
        {
            if (camId == IntPtr.Zero)
                return;

            uint nRet = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_StartStream(camId, 1, (uint)handle);
        }
        public void StopVideo(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return;

            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_StopStream(camId);
        }

        public void BackTrigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;

            if (backFlag.ContainsKey(camId))
                backFlag[camId] = index;
            else
                backFlag.Add(camId, index);

            uint nRet = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_TriggerExt(camId);
        }

        public void Trigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;

            uint nRet = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_TriggerExt(camId);
        }

        public void Send485(IntPtr camId, byte[] data)
        {
            if (camId == IntPtr.Zero)
                return;

            ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_TransSerialPort(camId, data, (uint)data.Length);
        }
    }
}
