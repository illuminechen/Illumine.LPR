
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Illumine.LPR
{
    public class ICECameraService : ICameraService
    {
        private static Dictionary<IntPtr, int> backFlag = new Dictionary<IntPtr, int>();

        private static ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate onPlate;
        private static ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate backSDK_onPlate;

        private static Action<LPRCameraArgs> Callback;
        private static Action<LPRCameraArgs> BackCallback;

        public void CheckValid(ref CameraViewerViewModel cvvm)
        {
            StringBuilder strResult = new StringBuilder(4096);
            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_GetDevID(cvvm.camId, strResult);//获取相机mac地址
            string rawMacAddress = strResult.ToString();

            string macAddress = string.Join("-", Enumerable.Range(0, rawMacAddress.Length / 2)
            .Select(i => rawMacAddress.Substring(i * 2, Math.Min(rawMacAddress.Length - i * 2, 2))));

            byte[] recvData = new byte[256];
            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_ReadUserData(cvvm.camId, recvData, 256);//读取数据
            
            string secret = Container.Get<Encoding>().GetString(recvData).Trim('\0');

            cvvm.IsValid = LicenseService.CheckCamera(secret, macAddress);
        }

        public bool Connect(ref CameraViewerViewModel cameraViewerViewModel)
        {
            IntPtr ret = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OpenDevice(cameraViewerViewModel.ChannelViewModel.Url);
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
            IntPtr ret = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OpenDevice(cameraViewerViewModel.ChannelViewModel.BackUrl);
            if (IntPtr.Zero == ret)
            {
                return false;
            }

            cameraViewerViewModel.backCamId = ret;
            return true;
        }

        public void Init()
        {
            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_Init();
        }

        public bool OpenDoor(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return false;

            uint nRet = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OpenGate(camId);
            return nRet == 1;
        }

        public void SetCallback(CameraViewerViewModel cvvm, Action<LPRCameraArgs> Callback)
        {
            if (ICECameraService.Callback == null)
                ICECameraService.Callback = Callback;

            if (onPlate == null)
                onPlate = new ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate(SDK_OnPlate);

            if (cvvm.ChannelViewModel.Enabled && cvvm.ChannelViewModel.CameraType == CameraType.Ice)
            {
                ICECameraServiceBase.ipcsdk.ICE_IPCSDK_SetPlateCallback(cvvm.camId, onPlate, new IntPtr(cvvm.ChannelId));//设置获取车牌识别数据的回调函数
            }
        }

        public void SetBackCallback(CameraViewerViewModel cvvm, Action<LPRCameraArgs> Callback)
        {
            if (ICECameraService.BackCallback == null)
                ICECameraService.BackCallback = Callback;

            if (backSDK_onPlate == null)
                backSDK_onPlate = new ICECameraServiceBase.ipcsdk.ICE_IPCSDK_OnPlate(BackSDK_OnPlate);

            if (cvvm.ChannelViewModel.Enabled && cvvm.ChannelViewModel.BackCameraType == CameraType.Ice)
            {
                ICECameraServiceBase.ipcsdk.ICE_IPCSDK_SetPlateCallback(cvvm.backCamId, backSDK_onPlate, cvvm.backCamId);
            }
        }
        public void BackSDK_OnPlate(System.IntPtr pvParam,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcNumber,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcColor,
            System.IntPtr pcPicData, uint u32PicLen, System.IntPtr pcCloseUpPicData, uint u32CloseUpPicLen,
            short nSpeed, short nVehicleType, short nReserved1, short nReserved2,
            float fPlateConfidence, uint u32VehicleColor, uint u32PlateType, uint u32VehicleDir, uint u32AlarmType,
            uint u32SerialNum, uint uCapTime, uint u32ResultHigh, uint u32ResultLow)
        {

            if (!backFlag.TryGetValue(pvParam, out int index))
            {
                return;
            }
            backFlag.Remove(pvParam);
            on_plate(pcIP, pcNumber, pcColor, pcPicData, u32PicLen, pcCloseUpPicData, u32CloseUpPicLen,
                nSpeed, nVehicleType, nReserved1, nReserved2, fPlateConfidence,
                u32VehicleColor, u32PlateType, u32VehicleDir, u32AlarmType, 0, index, u32ResultHigh, u32ResultLow, true);
        }

        public void SDK_OnPlate(System.IntPtr pvParam,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcNumber,
            [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcColor,
            System.IntPtr pcPicData, uint u32PicLen, System.IntPtr pcCloseUpPicData, uint u32CloseUpPicLen,
            short nSpeed, short nVehicleType, short nReserved1, short nReserved2,
            float fPlateConfidence, uint u32VehicleColor, uint u32PlateType, uint u32VehicleDir, uint u32AlarmType,
            uint u32SerialNum, uint uCapTime, uint u32ResultHigh, uint u32ResultLow)
        {
            int index = (int)pvParam;
            on_plate(pcIP, pcNumber, pcColor, pcPicData, u32PicLen, pcCloseUpPicData, u32CloseUpPicLen,
                nSpeed, nVehicleType, nReserved1, nReserved2, fPlateConfidence,
                u32VehicleColor, u32PlateType, u32VehicleDir, u32AlarmType, 0, index, u32ResultHigh, u32ResultLow, false);
        }

        public void on_plate(string bstrIP, string bstrNumber, string bstrColor, IntPtr vPicData, UInt32 nPicLen,
         IntPtr vCloseUpPicData, UInt32 nCloseUpPicLen, short nSpeed, short nVehicleType, short nReserved1, short nReserved2, Single fPlateConfidence,
         UInt32 nVehicleColor, UInt32 nPlateType, UInt32 nVehicleDir, UInt32 nAlarmType, UInt32 nCapTime, Int32 index, uint u32ResultHigh, uint u32ResultLow,
         bool back)
        {
#if VERSION32
            IntPtr vdcPtr = (IntPtr)u32ResultLow;
#else
            ulong tmp = ((ulong)u32ResultHigh << 32) + (ulong)u32ResultLow;
            IntPtr vdcPtr = (IntPtr)tmp;
#endif
            ICECameraServiceBase.ICE_VDC_PICTRUE_INFO_S vdcInfo = new ICECameraServiceBase.ICE_VDC_PICTRUE_INFO_S();

            if (vdcPtr != IntPtr.Zero)
            {
                //将数据拷贝到ICE_VDC_PICTRUE_INFO_S结构体
                vdcInfo = (ICECameraServiceBase.ICE_VDC_PICTRUE_INFO_S)Marshal.PtrToStructure(vdcPtr, typeof(ICECameraServiceBase.ICE_VDC_PICTRUE_INFO_S));
            }

            byte[] datajpg2 = null;

            if (nPicLen > 0)//全景图数据长度不为0
            {
                IntPtr ptr2 = (IntPtr)vPicData;
                datajpg2 = new byte[nPicLen];
                Marshal.Copy(ptr2, datajpg2, 0, datajpg2.Length);//拷贝图片数据
            }

            int left = vdcInfo.stPlateInfo.stPlateRect.s16Left;
            int right = vdcInfo.stPlateInfo.stPlateRect.s16Right;
            int top = vdcInfo.stPlateInfo.stPlateRect.s16Top;
            int bottom = vdcInfo.stPlateInfo.stPlateRect.s16Bottom;

            if (bstrNumber == "拸齪陬")
                bstrNumber = "";
            bstrNumber = bstrNumber == "" ? Container.Get<LPRSetting>().NoPlate : bstrNumber;

            if (back)
                BackCallback(new LPRCameraArgs(bstrNumber, new Rectangle(left, top, Math.Abs(right - left), Math.Abs(bottom - top)), datajpg2, index));
            else
                Callback(new LPRCameraArgs(bstrNumber, new Rectangle(left, top, Math.Abs(right - left), Math.Abs(bottom - top)), datajpg2, index, true));
        }
        public void StartVideo(IntPtr camId, IntPtr handle)
        {
            if (camId == IntPtr.Zero)
                return;

            uint nRet = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_StartStream(camId, 1, (uint)handle);
        }

        public void StopVideo(IntPtr camId)
        {
            if (camId == IntPtr.Zero)
                return;

            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_StopStream(camId);
        }

        //public void Trigger(IntPtr camId, int index)
        //{
        //    if (camId == IntPtr.Zero)
        //        return;

        //    backFlag.Add(camId, index);

        //    uint nRet = ICE_engCameraServiceBase.ipcsdk.ICE_IPCSDK_TriggerExt(camId);
        //}
        public void BackTrigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;
            if (backFlag.ContainsKey(camId))
                backFlag[camId] = index;
            else
                backFlag.Add(camId, index);

            StringBuilder strNum = new StringBuilder(32);
            StringBuilder strColor = new StringBuilder(64);
            uint len = 0;
            //IntPtr pLen = Marshal.AllocHGlobal(32);
            byte[] pdata = new byte[1048576];
            uint success = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_Trigger(camId, strNum, strColor, pdata, 1048576, ref len);//软触发

            byte[] datajpg2 = new byte[len];

            if (1 == success && len > 0)
            {
                Array.Copy(pdata, 0, datajpg2, 0, datajpg2.Length);//拷贝图片数据
            }
            string bstrNumber = strNum.ToString().Trim('\0');
            if (bstrNumber == "拸齪陬")
                bstrNumber = Container.Get<LPRSetting>().NoPlate;
            Callback(new LPRCameraArgs(bstrNumber, new Rectangle(), datajpg2, index));

            pdata = null;
            strNum = null;
            strColor = null;
        }

        public void Trigger(IntPtr camId, int index)
        {
            if (camId == IntPtr.Zero)
                return;

            StringBuilder strNum = new StringBuilder(32);
            StringBuilder strColor = new StringBuilder(64);
            uint len = 0;
            //IntPtr pLen = Marshal.AllocHGlobal(32);
            byte[] pdata = new byte[1048576];
            uint success = ICECameraServiceBase.ipcsdk.ICE_IPCSDK_Trigger(camId, strNum, strColor, pdata, 1048576, ref len);//软触发

            byte[] datajpg2 = new byte[len];

            if (1 == success && len > 0)
            {
                Array.Copy(pdata, 0, datajpg2, 0, datajpg2.Length);//拷贝图片数据
            }
            string bstrNumber = strNum.ToString().Trim('\0');
            if (bstrNumber == "拸齪陬")
                bstrNumber = Container.Get<LPRSetting>().NoPlate;
            Callback(new LPRCameraArgs(bstrNumber, new Rectangle(), datajpg2, index));

            pdata = null;
            strNum = null;
            strColor = null;
        }

        public void Send485(IntPtr camId, byte[] data)
        {
            if (camId == IntPtr.Zero)
                return;

            ICECameraServiceBase.ipcsdk.ICE_IPCSDK_TransSerialPort(camId, data, (uint)data.Length);
        }
    }
}
