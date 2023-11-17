using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Illumine.LPR
{
    public static class Model1CameraServiceBase
    {
        public const int MAX_HOST_LEN = 16;
        public const int ONE_DIRECTION_LANES = 5;
        public const int VERSION_NAME_LEN = 64;

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_Init();

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void Net_UNinit();

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_AddCamera(string ptIp);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_DelCamera(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_ConnCamera(int tHandle, ushort usPort, ushort usTimeout);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_DisConnCamera(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StartVideo(int tHandle, int niStreamType, IntPtr hWnd);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StopVideo(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_GetSdkVersion([MarshalAs(UnmanagedType.LPStr)] StringBuilder szVersion, ref int ptLen);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_ReadGPIOState(int tHandle, byte ucIndex, ref byte pucValue);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_WriteGPIOState(int tHandle, byte ucIndex, byte ucValue);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern float Net_MatchSpecialCode2(float[] fSpecialCode1, float[] fSpecialCode2);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegImageRecv(FGetImageCB fCb);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegImageRecv2(FGetImageCB2 fCb);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegImageRecvEx(
          int tHandle,
          FGetImageCBEx fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegImageRecvEx2(
          int tHandle,
          FGetImageCBEx2 fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegOffLineImageRecvEx(
          int tHandle,
          FGetOffLineImageCBEx fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegReportMessEx(
          int tHandle,
          FGetReportCBEx fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegOffLineClient(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_ImageSnap(int tHandle, ref T_FrameInfo ptImageSnap);

        [DllImport(@".\Model1\NetSDK.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_GateSetup(
          int tHandle,
          ref T_ControlGate ptControlGate);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_BlackWhiteListSend(
          int tHandle,
          ref T_BlackWhiteList ptBalckWhiteList);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_GetBlackWhiteList(
          int tHandle,
          ref T_GetBlackWhiteList ptGetBalckWhiteList);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_GetBlackWhiteListAsCSV(
          int tHandle,
          ref T_GetBlackWhiteList ptGetBalckWhiteList);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_BlackWhiteListSetup(
          ref T_LprResult ptLprResult,
          ref T_BlackWhiteListCount ptBlackWhiteListCount);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SaveImageToJpeg(int tHandle, string ucPathNmme);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_GetJpgBuffer(
          int tHandle,
          ref IntPtr ucJpgBuffer,
          ref ulong ulSize);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_FreeBuffer(IntPtr pJpgBuffer);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SaveJpgFile(int tHandle, string strJpgFile);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SaveBmpFile(int tHandle, string strBmpFile);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StartRecord(int tHandle, string strFile);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StopRecord(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_ShowPlateRegion(int tHandle, int niShowMode);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_UpdatePlateRegion(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegOffLinePayRecord(
          int tHandle,
          FGetOffLinePayRecordCB fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_NETSetup(int tHandle, ref T_NetSetup ptNetSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryNETSetup(int tHandle, ref T_NetSetup ptNetSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SetDrawFunCallBack(
          int tHandle,
          FDrawFunCallBack fCb,
          IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_ReadTwoEncpyption(
          int tHandle,
          [MarshalAs(UnmanagedType.LPStr)] StringBuilder pBuff,
          uint uiSizeBuff);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_WriteTwoEncpyption(int tHandle, string pUserData, uint uiDataLen);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_VideoDetectSetup(
          int tHandle,
          ref T_VideoDetectParamSetup ptVideoDetectParamSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryVideoDetectSetup(
          int tHandle,
          ref T_VideoDetectParamSetup ptVideoDetectParamSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SendRS485Data(
          int tHandle,
          ref T_RS485Data ptRS485Data);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_TimeSetup(int tHandle, ref T_DCTimeSetup ptTimeSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryTimeSetup(
          int tHandle,
          ref T_DCTimeSetup ptTimeSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_VehicleVAFunSetup(
          int tHandle,
          ref T_VehicleVAFunSetup ptVehicleVAFunSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryVehicleVAFunSetup(
          int tHandle,
          ref T_VehicleVAFunSetup ptVehicleVAFunSetup);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_FindDevice(FNetFindDeviceCallback fCb, IntPtr obj);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StartTalk(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_StopTalk(int tHandle);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SendBlackWhiteListByMess(
          int tHandle,
          ref T_SendLprByMess ptSendLprByMes);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_SetParkOpenManual(
          int tHandle,
          ref T_ParkOpenManual ptParkOpenManual);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryParkOpenManual(
          int tHandle,
          ref T_ParkOpenManual ptParkOpenManual);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_AudioTalkBack(
          int tHandle,
          ref T_AudioTalkBack ptAudioTalkBack);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_QueryAudioLinkinfo(
          int tHandle,
          ref T_AudioLinkinfo ptAudioLinkinfo);

        [DllImport(@".\Model1\NetSDK.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.StdCall)]
        public static extern int Net_RegTalkConnStateCallBack(
          FTalkConnStateCallBack fCb,
          IntPtr obj);

        public struct T_ImageUserInfo
        {
            public ushort usWidth;
            public ushort usHeight;
            public byte ucVehicleColor;
            public byte ucVehicleBrand;
            public byte ucVehicleSize;
            public byte ucPlateColor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] szLprResult;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] usLpBox;
            public byte ucLprType;
            public ushort usSpeed;
            public byte ucSnapType;
            public byte ucReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] acSnapTime;
            public byte ucViolateCode;
            public byte ucLaneNo;
            public uint uiVehicleId;
            public byte ucScore;
            public byte ucDirection;
            public byte ucTotalNum;
            public byte ucSnapshotIndex;
        }

        public struct T_ImageUserInfo2
        {
            public ushort usWidth;
            public ushort usHeight;
            public byte ucVehicleColor;
            public byte ucVehicleBrand;
            public byte ucVehicleSize;
            public byte ucPlateColor;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] szLprResult;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public ushort[] usLpBox;
            public byte ucLprType;
            public ushort usSpeed;
            public byte ucSnapType;
            public byte ucReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] acSnapTime;
            public byte ucViolateCode;
            public byte ucLaneNo;
            public uint uiVehicleId;
            public byte ucScore;
            public byte ucDirection;
            public byte ucTotalNum;
            public byte ucSnapshotIndex;
            public uint uiVideoProcTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] strVehicleBrand;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] strConfidence;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 240)]
            public byte[] strSpecialCode;
            public byte ucHasCar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aucReserved1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 252)]
            public byte[] aucReserved2;
        }

        public struct T_PicInfo
        {
            public uint uiPanoramaPicLen;
            public uint uiVehiclePicLen;
            public IntPtr ptPanoramaPicBuff;
            public IntPtr ptVehiclePicBuff;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetImageCB(
          int tHandle,
          uint uiImageId,
          ref T_ImageUserInfo tImageInfo,
          ref T_PicInfo tPicInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetImageCB2(
          int tHandle,
          uint uiImageId,
          ref T_ImageUserInfo2 tImageInfo,
          ref T_PicInfo tPicInfo);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetImageCBEx(
          int tHandle,
          uint uiImageId,
          ref T_ImageUserInfo tImageInfo,
          ref T_PicInfo tPicInfo,
          IntPtr obj);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetImageCBEx2(
          int tHandle,
          uint uiImageId,
          ref T_ImageUserInfo2 tImageInfo,
          ref T_PicInfo tPicInfo,
          IntPtr obj);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetOffLineImageCBEx(
          int tHandle,
          uint uiImageId,
          ref T_ImageUserInfo tImageInfo,
          ref T_PicInfo tPicInfo,
          IntPtr obj);

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetReportCBEx(
          int tHandle,
          byte ucType,
          IntPtr ptMessage,
          IntPtr pUserData);

        public struct T_FrameInfo
        {
            public uint uiFrameId;
            public uint uiTimeStamp;
            public uint uiEncSize;
            public uint uiFrameType;
        }

        public struct T_ControlGate
        {
            [MarshalAs(UnmanagedType.U1)]
            public byte ucState;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] ucReserved;
        }

        public struct T_BlackWhiteList
        {
            public byte LprMode;
            public byte LprCode;
            public byte Lprnew;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] aucLplPath;
        }

        public struct T_GetBlackWhiteList
        {
            public byte LprMode;
            public byte LprCode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] aucLplPath;
        }

        public struct T_LprResult
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] LprResult;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] StartTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] EndTime;
        }

        public struct T_BlackWhiteListCount
        {
            public int uiCount;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] aucLplPath;
        }

        public struct T_VehPayRsp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] acPlate;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] acEntryTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
            public byte[] acExitTime;
            public uint uiRequired;
            public uint uiPrepaid;
            public byte ucVehType;
            public byte ucUserType;
            public byte ucResultCode;
            public byte acReserved;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FGetOffLinePayRecordCB(
          int tHandle,
          byte ucType,
          ref T_VehPayRsp ptVehPayInfo,
          uint uiLen,
          ref T_PicInfo ptPicInfo,
          IntPtr obj);

        public struct T_NetSetup
        {
            public byte ucEth;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aucReserved;
            public uint uiIPAddress;
            public uint uiMaskAddress;
            public uint uiGatewayAddress;
            public uint uiDNS1;
            public uint uiDNS2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] szHostName;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FDrawFunCallBack(
          int tHandle,
          IntPtr hdc,
          int width,
          int height,
          IntPtr obj);

        public struct T_Point
        {
            public short sX;
            public short sY;
        }

        public struct T_Line
        {
            public T_Point tStartPoint;
            public T_Point tEndPoint;
        }

        public struct T_Rect
        {
            public T_Point tLefTop;
            public T_Point tRightBottom;
        }

        public struct T_DivisionLine
        {
            public byte ucDashedLine;
            public byte ucReserved;
            public T_Line tLine;
        }

        public struct T_VideoDetectParamSetup
        {
            public byte ucLanes;
            public byte ucSnapAutoBike;
            public ushort usBanTime;
            public byte ucVirLoopNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aucReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.Struct)]
            public T_Point[] atPlateRegion;
            public T_Line aStopLine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.Struct)]
            public T_Point[] atSpeedRegion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2, ArraySubType = UnmanagedType.Struct)]
            public T_Line[] atOccupCheckLine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6, ArraySubType = UnmanagedType.Struct)]
            public T_DivisionLine[] atDivisionLine;
            public T_Line tPrefixLine;
            public T_Line tLeftBorderLine;
            public T_Line tRightBorderLine;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20, ArraySubType = UnmanagedType.Struct)]
            public T_Point[] atVirLoop;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4, ArraySubType = UnmanagedType.Struct)]
            public T_Point[] atBanRegion;
            public ushort usCameraHeight;
            public ushort usRectLength;
            public ushort usRectWidth;
            public ushort usTotalDis;
        }

        public struct T_RS485Data
        {
            public byte rs485Id;
            public byte ucReserved;
            public ushort dataLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public byte[] data;
        }

        public struct T_DCTimeSetup
        {
            public ushort usYear;
            public byte ucMonth;
            public byte ucDay;
            public byte ucHour;
            public byte ucMinute;
            public byte ucSecond;
            public byte ucDayFmt;
            public byte ucTimeFmt;
            public byte ucTimeZone;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] aucReserved;
        }

        public struct T_VehicleVAFunSetup
        {
            public byte ucPlateRecogEn;
            public byte ucVehicleSizeRecogEn;
            public byte ucVehicleColorRecogEn;
            public byte ucVehicleBrandRecogEn;
            public byte ucVehicleSizeClassify;
            public byte ucLocalCity;
            public byte ucPlateDirection;
            public byte ucCpTimeInterval;
            public uint uiPlateDefaultWord;
            public ushort usMinPlateW;
            public ushort usMaxPlateW;
            public byte ucDoubleYellowPlate;
            public byte ucDoubleArmyPlate;
            public byte ucPolicePlate;
            public byte ucPlateFeature;
        }

        public struct T_MACSetup
        {
            public byte ucEth;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aucReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] aucMACAddresss;
        }

        public struct T_RcvMsg
        {
            public uint uiFlag;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] aucDstMACAdd;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] aucAdapterName;
            public uint uiAdapterSubMask;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] ancDevType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] ancSerialNum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] ancAppVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] ancDSPVersion;
            public T_NetSetup tNetSetup;
            public T_MACSetup tMacSetup;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate int FNetFindDeviceCallback(ref T_RcvMsg ptFindDevice, IntPtr obj);

        public struct T_QueVersionRsp
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szKernelVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szFileSystemVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szAppVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szWebVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szHardwareVersion;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szDevType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szSerialNum;
            public uint uiDateOfExpiry;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] szDSPVersion;
        }

        public struct T_SendLprByMess
        {
            public byte ucType;
            public byte ucConut;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] aucReserved;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public T_LprResult[] atLprResult;
        }

        public struct T_ParkLedManualItem
        {
            public byte ucEnable;
            public byte ucLevel;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] aucReserved;
            public ushort usStartTime;
            public ushort usEndTime;
        }

        public struct T_ParkOpenManual
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public T_ParkLedManualItem[] atParkLedMan;
        }

        public struct T_AudioLinkInfo
        {
            public int iIpAddr;
            public byte ucStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 127)]
            public byte[] Reserved;
        }

        public struct T_AudioTalkBack
        {
            public byte enable;
            public byte pressstatus;
            public byte ucOutTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] Reserved;
        }

        public struct T_AudioLinkinfo
        {
            public int iIpAddr;
            public byte ucStatus;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 127)]
            public byte[] Reserved;
        }

        [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public delegate void FTalkConnStateCallBack(int tHandle, byte ucCtrlConnState, IntPtr obj);
    }
}
