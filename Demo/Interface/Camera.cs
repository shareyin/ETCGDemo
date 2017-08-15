//////////////////////////////////////////////////////////////////////////
//Commit:   对车牌识别动态库进行C#代码的封装，统一调用接口，
//          兼容海康威视高清和信路威等标清车牌识别
//Author:   DQS
//Date：    2012-07-25
//Version:  1.0
//////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace ETCF
{
    public class Camera
    {
        string PlateNo = "";                                //Plate
        byte[] PlateNoBuf = new byte[20];                   //Buffer for plate
        public byte[] VehImage = new byte[524288];          //buffer for Vehicle image, max 512k
        public byte[] PlateImage = new byte[102400];        //buffer for plate image, max 100k
        public byte[] BinaryImage = new byte[102400];       //buffer for binary plate image, max 100k
        public byte[] FarBigImage = new byte[524288];       //buffer for far big image, max 512k
        public int VehImageLen = 0;                         //length of the Vehicle image
        public int PlateImageLen = 0;                       //length of the plate image
        public int BinaryImageLen = 0;                      //length of the binary plate image 
        public int FarBigImageLen = 0;                      //length of the far bif image

        //-------------------declaration of Fcuntion of dll-------------------
        [DllImport("WJCamera.dll", EntryPoint = "WVS_Initialize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_Initialize();

        [DllImport("WJCamera.dll", EntryPoint = "WVS_CloseHv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_CloseHv();

        [DllImport("WJCamera.dll", EntryPoint = "WVS_OpenOneLaneHv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_OpenOneLaneHv(int LaneNo);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_CloseOneLaneHv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_CloseOneLaneHv(int LaneNo);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_ForceSendLaneHv", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_ForceSendLaneHv(int Laneno, int ImageOutType);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetBigImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetBigImage(
            byte LaneNo,
            int IdentNo,
            ref byte ImgBuf,
            int ImgBufLen,
            ref int ImgSize);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetBinaryImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetBinaryImage(
            byte LaneNo,
            int IdentNo,
            ref byte ImgBuf,
            int ImgBufLen,
            ref int ImgSize);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetFarBigImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetFarBigImage(
            byte LaneNo,
            int IdentNo,
            ref byte ImgBuf,
            int ImgBufLen,
            ref int ImgSize);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetHvIsConnected", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetHvIsConnected(int LaneNo);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetLandNoCount", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetLandNoCount();

        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetPlateNo", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetPlateNo(
            byte LaneNo,
            int IdentNo,
            ref byte PlateNo,
            int PlateNoLen);
        [DllImport("WJCamera.dll", EntryPoint = "WVS_GetSmallImage", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_GetSmallImage(
            byte LaneNo,
            int IdentNo,
            ref byte Imgbuf,
            int ImgBufLen,
            ref int ImgSize);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_Settime", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_Settime(int LaneNo);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_StartRealPlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_StartRealPlay(int LaneNo, IntPtr hHwnd);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_Startrecord", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_Startrecord(int LaneNO, string FileName);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_StopRealPlay", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_StopRealPlay(int LaneNo);

        [DllImport("WJCamera.dll", EntryPoint = "WVS_Stoprecord", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WVS_Stoprecord(int LaneNo);
        //--------------------------------------------------------------------
        //--------------------------------------------------------------------
        //Function:  Initialize the Camera
        //In: void       
        //Out: int, 0 for success. the fail number use 8 byte to show which 
        //     camera init failed; e.g.: 5 is 00000101, the first and third
        //     camera init failed.
        //--------------------------------------------------------------------
        public int Initialize()
        {
            int val;
            val = WVS_Initialize();
            return val;
        }

        //--------------------------------------------------------------------
        //Function:  Close all the cameras
        //In:  void
        //Out: int, 0 for success. the fail number use 8 byte to show which 
        //     camera close failed; e.g.: 5 is 00000101, the first and third
        //     camera close failed.
        //--------------------------------------------------------------------
        public int Close()
        {
            int val;
            val = WVS_CloseHv();
            return val;
        }

        //--------------------------------------------------------------------
        //Function:  Initialize the single Camera
        //In: int
        //Out: int, 0 for success, 1 for fail
        //--------------------------------------------------------------------
        public int InitOneLaneDev(int LaneNo)
        {
            int val;
            val = WVS_OpenOneLaneHv(LaneNo);
            return val;
        }

        //--------------------------------------------------------------------
        //Function:  Close the single Camera
        //In: int
        //Out: int, o for success, 1 for fail
        //--------------------------------------------------------------------
        public int CloseOneLaneDev(int LaneNo)
        {
            int val;
            val = WVS_CloseOneLaneHv(LaneNo);
            return val;
        }

        //--------------------------------------------------------------------
        //Function: Force the Camera to Identify the plate
        //In:  LaneNo:  int, the No of the Lane
        //     ImageOutType: int, the type of the out image
        //Out: int, 0 for success, -1 for fail
        //--------------------------------------------------------------------
        public int ForceGetLanePlate(int LaneNo)
        {
            int val, ImgOutType = 4;
            val = WVS_ForceSendLaneHv(LaneNo, ImgOutType);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Get the PlateNo of the appoint lane and identno Vehicle
        //IN:   LaneNo, byte
        //      IdentNo, int
        //      PlateNo, string
        //      PlateNoLen, int
        //OUT:  plateno
        //--------------------------------------------------------------------
        public string GetPlateNo(byte LaneNo, int IdentNo)
        {
            int val;
            PlateNo = "";
            //此处动态库函数有bug，buf没有清空，导致车牌为'未检测'时，后面会残留上次车牌的尾号
            //如上次车牌号为鄂J02392,本次为‘未检测’，则动态库返回 ‘未检测\0392\0\0\0\0\0\0\0\0’,
            //由于调用web service，不能带\0字符，故原想把\0都去掉，结果就变成了 ‘未检测392’
            val = WVS_GetPlateNo(LaneNo, IdentNo, ref PlateNoBuf[0], PlateNoBuf.Length);
            if (-1 == val)
                PlateNo = "";
            else
            {
                //PlateNo = System.Text.Encoding.GetEncoding("GB2312").GetString(PlateNoBuf, 0, PlateNoBuf.Length).Replace("\0", "");
                PlateNo = System.Text.Encoding.GetEncoding("GB2312").GetString(PlateNoBuf, 0, PlateNoBuf.Length);
                PlateNo = PlateNo.Substring(0, PlateNo.IndexOf("\0"));
            }
            return PlateNo;
        }

        //--------------------------------------------------------------------
        //Function:  Get the image of appoint lane and identno Vehicle
        //IN:  LaneNo: byte, the No of the Lane
        //     IdentNo: int
        //     ImgBuf: byte[] 
        //     ImgBufLen: int
        //     ImgSize: int
        //OUT: int
        //      0: success
        //      -1: failed, invalid LaneNo
        //      -2: failed, invalid point of Imgbuf
        //      -3: failed, the ImgBufLen is null
        //      -4: failed, the buf of image is not enough
        //--------------------------------------------------------------------
        public int GetBigImage(
            byte LaneNo,
            int IdentNo)
        {
            int val;
            VehImageLen = 0;
            for (int i = 0; i < VehImage.Length; i++)
                VehImage[i] = 0;
            val = WVS_GetBigImage(LaneNo, IdentNo, ref VehImage[0], VehImage.Length, ref VehImageLen);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: GET the image of plate
        //IN:  LaneNo: byte, the No of the Lane
        //     IdentNo: int
        //     ImgBuf: sbyte[] 
        //     ImgBufLen: int
        //     ImgSize: int
        //OUT: int
        //      0: success
        //      -1: failed, invalid LaneNo
        //      -2: failed, invalid point of Imgbuf
        //      -3: failed, the ImgBufLen is null
        //      -4: failed, the buf of image is not enough
        //--------------------------------------------------------------------  
        public int GetPlateImage(
            byte LaneNo,
            int IdentNo)
        {
            int val;
            PlateImageLen = 0;
            for (int i = 0; i < PlateImage.Length; i++)
                PlateImage[i] = 0;
            val = WVS_GetSmallImage(LaneNo, IdentNo, ref PlateImage[0], PlateImage.Length, ref PlateImageLen);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Get Far image of appoint lane and identno Vehicle
        //IN:  LaneNo: byte, the No of the Lane
        //     IdentNo: int
        //     ImgBuf: sbyte[] 
        //     ImgBufLen: int
        //     ImgSize: int
        //OUT: int
        //      0: successed
        //      -1: failed, invalid LaneNo
        //      -2: failed, invalid point of Imgbuf
        //      -3: failed, the ImgBufLen is null
        //      -4: failed, the buf of image is not enough
        //--------------------------------------------------------------------
        public int GetFarBigImage(
            byte LaneNo,
            int IdentNo)
        {
            int val;
            FarBigImageLen = 0;
            for (int i = 0; i < FarBigImage.Length; i++)
                FarBigImage[i] = 0;
            val = WVS_GetFarBigImage(LaneNo, IdentNo, ref FarBigImage[0], FarBigImage.Length, ref FarBigImageLen);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Get the binary image of appoint lane and identno Vehicle
        //IN:  LaneNo: byte, the No of the Lane
        //     IdentNo: int
        //     ImgBuf: sbyte[] 
        //     ImgBufLen: int
        //     ImgSize: int
        //OUT: int
        //      0: successed
        //      -1: failed, invalid LaneNo
        //      -2: failed, invalid point of Imgbuf
        //      -3: failed, the ImgBufLen is null
        //      -4: failed, the buf of image is not enough
        //-------------------------------------------------------------------- 
        public int GetBinaryPlateImage(
            byte LaneNo,
            int IdentNo)
        {
            int val;
            BinaryImageLen = 0;
            for (int i = 0; i < BinaryImage.Length; i++ )
                BinaryImage[i] = 0;
            val = WVS_GetBinaryImage(LaneNo, IdentNo, ref BinaryImage[0], BinaryImage.Length, ref BinaryImageLen);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION:  Use the Local Time to Set Time of the Camera
        //IN: LaneNO, int
        //OUT:  0 for success, -1 for failed
        //--------------------------------------------------------------------
        public int SetDateTime(int LaneNo)
        {
            int val;
            val = WVS_Settime(LaneNo);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Start the real video to play
        //IN: LaneNo, int ;  hHwnd, the handle of the container
        //OUT:  1 for success, 0 failed
        //--------------------------------------------------------------------
        public int StartRealVedioPlay(int LaneNo, IntPtr hHwnd)
        {
            int val;
            val = WVS_StartRealPlay(LaneNo, hHwnd);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Stop the real video play
        //IN:   LaneNo, int
        //OUT:  1 for success, 0 for failed.
        //--------------------------------------------------------------------        
        public int StopRealVedioPlay(int LaneNo)
        {
            int val;
            val = WVS_StopRealPlay(LaneNo);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Start record the video
        //IN: LaneNo, int; FileName, string;
        //OUT:  1 for success
        //      0: fail to open the file
        //      -1: the record is aleady started
        //      -2: the record is failed
        //      -3: the real video is not open
        //--------------------------------------------------------------------
        public int StartVedioRecord(int LaneNO, string FileName)
        {
            int val;
            val = WVS_Startrecord(LaneNO, FileName);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION: Stop the record
        //IN:   LaneNo, int
        //OUT:  1 for success
        //      -1: the record hasn't started
        //      0: stop the record failed.
        //--------------------------------------------------------------------
        public int StopVedioRecord(int LaneNo)
        {
            int val;
            val = WVS_Stoprecord(LaneNo);
            return val;

        }

        //--------------------------------------------------------------------
        //FUNCTION: Test the Camera if Connected.
        //IN:  LaneNo, int
        //OUT: int, 0 is connected, -1 is unconnected
        //--------------------------------------------------------------------
        public int TestIsConnect(int LaneNo)
        {
            int val;
            val = WVS_GetHvIsConnected(LaneNo);
            return val;
        }

        //--------------------------------------------------------------------
        //FUNCTION:  Get the number of lane
        //IN: void
        //OUT: int
        //--------------------------------------------------------------------
        public int GetDevNumber()
        {
            int val;
            val = WVS_GetLandNoCount();
            return val;
        }
    }
}