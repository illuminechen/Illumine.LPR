using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace Illumine.LPR
{
    public static class ICE_engCameraServiceBase
    {
        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct ICE_RECT_S
        {

            /// ICE_S16->short
            public short s16Left;

            /// ICE_S16->short
            public short s16Top;

            /// ICE_S16->short
            public short s16Right;

            /// ICE_S16->short
            public short s16Bottom;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct T_TimeInfo
        {

            /// ICE_S32->int
            public int iYear;

            /// ICE_S32->int
            public int iMon;

            /// ICE_S32->int
            public int iDay;

            /// ICE_S32->int
            public int iHour;

            /// ICE_S32->int
            public int iMin;

            /// ICE_S32->int
            public int iSec;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct T_PlateInfo
        {

            /// char[128]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] plateNum;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string plateColor;

            /// int
            public int flasePlate;

            /// float
            public float confidence;

            /// int
            public int plateType;

            /// ICE_RECT_S->Anonymous_d925413f_05ad_469e_a327_919ffea37687
            public ICE_RECT_S stPlateRect;

            /// char[256]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 256)]
            public string resv;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct T_VehicleInfo
        {

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string vehicleType;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string vehicleColor;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string vehicleDir;

            /// char[128]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] logName;

            /// char[128]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 128)]
            public byte[] subLogName;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] productYearName;

            /// char[256]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 256)]
            public string resv;
        }

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential, CharSet = System.Runtime.InteropServices.CharSet.Ansi)]
        public struct T_LprResult
        {

            /// T_PlateInfo
            public T_PlateInfo plateInfo;

            /// T_VehicleInfo
            public T_VehicleInfo vehicleInfo;

            /// int
            public int lprState;

            /// int
            public int iHadPlate;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string triggerType;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string plateState;

            /// char[32]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 32)]
            public string bwList;

            /// T_TimeInfo
            public T_TimeInfo capTime;

            /// char[512]
            [System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.ByValTStr, SizeConst = 512)]
            public string resv;
        }

        public class ipcsdk
        {
            [System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute(System.Runtime.InteropServices.CallingConvention.StdCall)]
            public delegate void ICE_IPCSDK_OnPlate(System.IntPtr pvParam,
                [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIp,
                ref T_LprResult tLprResult, System.IntPtr pFullPicData, int fullPicLen, System.IntPtr pPlatePicData, int platePiclen, uint u32Reserved1, uint u32Reserved2);

            /**
            *  @brief  Set the plate recognition event callback
            *  @param  [IN] hSDK            SDK handle
            *  @param  [IN] pfOnPlate       the plate recognition event callback
            *  @param  [IN] pvParam         callback context
            *　@return void
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_SetPlateCallback", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_SetPlateCallback(System.IntPtr hSDK, ICE_IPCSDK_OnPlate pfOnPlate, System.IntPtr pvParam);

            /**
            *  @brief  Global initialization
            *  @return void
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_Init", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_Init();

            /**
            *  @brief  Global release
            *  @return void
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_Fini", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_Fini();

            /**
           *  @brief  Connection without video streaming
           *  @param  [IN] pcIP      ip address
           *  @return SDK handle(If the connection is unsuccessful, the return value is null)
           */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_OpenDevice", CallingConvention = CallingConvention.Cdecl)]
            public static extern System.IntPtr ICE_IPCSDK_OpenDevice([System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP);

            /**
            *  @brief  disconnect
            *  @param  [IN] hSDK    SDK handle
            *  return void
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_Close", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_Close(System.IntPtr hSDK);

            /**
            *  @brief  Open gate
            *  @param  [IN] hSDK  SDK handle
            *  @return 1 success 0 fail
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_OpenGate", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_OpenGate(System.IntPtr hSDK);

            /**
            *  @brief  start video
            *  @param  [IN] hSDK          SDK handle
            *  @param  [IN] u8MainStream  whether is main stream.1:main stream 0:subStream
            *  @param  [IN] hWnd          window handle of video playeback
            *  @return 1 success 0 fail
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_StartStream", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_StartStream(System.IntPtr hSDK, byte u8MainStream, uint hWnd);

            /**
            *  @brief  stop video
            *  @param  [IN] hSDK          SDK handle
            *  @return void
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_StopStream", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_StopStream(System.IntPtr hSDK);

            /**
            *  @brief  Software Trigger extension
            *  @param  [IN] hSDK          SDK handle 
            *  @return 0:fail 1 success 2:Identification in progress  3:Algorithm not started
            */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_TriggerExt", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_TriggerExt(System.IntPtr hSDK);

            public delegate void ICE_IPCSDK_OnDeviceEvent(System.IntPtr pvParam, System.IntPtr pvHandle,
                [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcIP,
                uint u32EventType, uint u32EventData1, uint u32EventData2, uint u32EventData3, uint u32EventData4);

            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_SetDeviceEventCallBack", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_SetDeviceEventCallBack(System.IntPtr hSDK, ICE_IPCSDK_OnDeviceEvent pfOnDeviceEvent, System.IntPtr pvDeviceEventParam);

            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_RegLprResult", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_RegLprResult(System.IntPtr hSDK, int iNeedPic, int iReserve);

            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_UnregLprResult", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_UnregLprResult(System.IntPtr hSDK);


            /**
             *  @brief  写入用户数据
             *  @param  [IN] hSDK       由连接相机接口获得的句柄
             *  @parame [IN] pcData     需要写入的用户数据
             *  @return 1表示成功，0表示失败
             */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_WriteUserData", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_WriteUserData(System.IntPtr hSDK,
                [System.Runtime.InteropServices.InAttribute()][System.Runtime.InteropServices.MarshalAsAttribute(System.Runtime.InteropServices.UnmanagedType.LPStr)] string pcData);

            /**
             *  @brief  读取用户数据
             *  @param  [IN] hSDK       由连接相机接口获得的句柄
             *  @parame [OUT] pcData    读取的用户数据
             *  @param  [IN] nSize      读出的数据的最大长度，即缓冲区大小
             *  @return 1表示成功，0表示失败
             */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_ReadUserData", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_ReadUserData(System.IntPtr hSDK, byte[] pcData, int nSize);

            /**
             *  @brief  搜索区域网内相机
             *  @param  [OUT] szDevs   设备mac地址和ip地址的字符串
             *                         设备mac地址和ip地址的字符串，格式为：mac地址 ip地址 例如：00-00-00-00-00-00 192.168.55.150\r\n
             *  @return void
             */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_SearchDev", CallingConvention = CallingConvention.Cdecl)]
            public static extern void ICE_IPCSDK_SearchDev(StringBuilder szDevs);

            /**
             *  @brief  获取相机mac地址
             *  @param  [IN] hSDK      由连接相机接口获得的句柄
             *  @param  [OUT] szDevID  相机mac地址
             *  @return 1表示成功，0表示失败
             */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_GetDevID", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint ICE_IPCSDK_GetDevID(System.IntPtr hSDK, StringBuilder szDevID);

            /**
             *  @brief  Send transparent serial port data(rs485)
             *  @param  [IN] hSDK      SDK handle
             *  @param  [IN] pcData    Serial data
             *  @param  [IN] u32Len    length of serial data
             *  @return 0 fail, 1 success
             */
            [System.Runtime.InteropServices.DllImportAttribute(@".\ICE_eng\ice_ipcsdk.dll", EntryPoint = "ICE_IPCSDK_TransSerialPort", CallingConvention = CallingConvention.Cdecl)]
            //public static extern uint ICE_IPCSDK_TransSerialPort(System.IntPtr hSDK, String pcData, uint u32Len);
            public static extern uint ICE_IPCSDK_TransSerialPort(System.IntPtr hSDK, byte[] pcData, uint u32Len);

        }
    }
}
