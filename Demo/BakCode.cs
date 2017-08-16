using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ETCF
{
    class BakCode
    {
        ////第一版本的数据匹配逻辑
        //public void QueueDataHander()
        //{
        //    bool isInRSUSql = false;
        //    string sZuobiString = "";
        //    int iJGCarType = 0;
        //    int iOBUCarType = 0;
        //    //bool isInJGSql = false;
        //    //bool isInAllSql = false;
        //    long bTempCode = 0x00000001;
        //    List<string> listPlateNum = new List<string>();//车牌缓存队列
        //    List<string> listTempCode = new List<string>();//随机码缓存队列

        //    //List<OBUList> listInfoOBU = new List<OBUList>();
        //    bool isGetbyCode = false;
        //    bool isGetbyPlate = false;
        //    //string sTempPlateNum = "";
        //    string InsertString = "";
        //    QueueRSUData qoutRSU = new QueueRSUData();
        //    QueueJGData qoutJG = new QueueJGData();
        //    while (true)
        //    {
        //        try
        //        {
        //            isGetbyCode = false;
        //            isGetbyPlate = false;
        //            iJGCarType = 0;
        //            iOBUCarType = 0;
        //            //先取ETC的数据
        //            if (qRSUData.TryDequeue(out qoutRSU))
        //            {
        //                //先缓存车牌
        //                listPlateNum.Add(qoutRSU.qOBUPlateNum);
        //                listTempCode.Add(qoutRSU.qRSURandCode.ToString("X2"));
        //                //sTempPlateNum=qoutRSU.qOBUPlateNum;
        //                //先进行RSU数据存储
        //                isInRSUSql = InsertRSUData(qoutRSU.qOBUPlateColor, qoutRSU.qOBUPlateNum, qoutRSU.qOBUMac, qoutRSU.qOBUY, qoutRSU.qOBUCarLength, qoutRSU.qOBUCarhigh, qoutRSU.qOBUCarType, qoutRSU.qOBUDateTime, qoutRSU.qRSURandCode.ToString("X2"));
        //                if (!isInRSUSql)
        //                {
        //                    //异常
        //                }
        //                else
        //                {
        //                    Log.WritePlateLog(DateTime.Now + " 天线数据跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库OBU表\r\n");
        //                    adddataGridViewRoll(qoutRSU.qCount, "", qoutRSU.qOBUCarType, qoutRSU.qOBUDateTime, "", qoutRSU.qOBUPlateNum, "", qoutRSU.qOBUPlateColor, "", "", "", "", "", "");
        //                }
        //                jg_inQueueDone.Reset();
        //                //查找激光是否已经加入缓存
        //                if (jg_inQueueDone.WaitOne(6000))
        //                {

        //                    //匹配
        //                    if (qJGData.TryDequeue(out qoutJG))
        //                    {
        //                        if (qoutJG.qJGCarType == "未知")
        //                        {
        //                            sZuobiString = "作弊未知";
        //                            iJGCarType = 0;
        //                            iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                        }
        //                        else
        //                        {
        //                            iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                            iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                            if (iJGCarType <= iOBUCarType)
        //                            {
        //                                sZuobiString = "正常通车";
        //                            }
        //                            else
        //                            {
        //                                sZuobiString = "可能作弊";
        //                            }
        //                        }
        //                        InsertJGData(qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qJGCarType, qoutJG.qJGId, qoutJG.qCamPlateNum, qoutJG.qCamPicPath, qoutJG.qJGDateTime, qoutJG.qCambiao, qoutJG.qCamPlateColor, qoutJG.qJGRandCode.ToString("X2"));
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据1跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库JG表\r\n");
        //                        if (qoutRSU.qOBUPlateNum == qoutJG.qCamPlateNum)
        //                        {
        //                            try
        //                            {
        //                                //车牌一次匹配成功
        //                                //列入总表
        //                                InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('" + qoutJG.qJGLength + "','" + qoutJG.qJGWide + "','" + qoutJG.qJGCarType + "','" + qoutJG.qJGDateTime + "','" + qoutJG.qCamPlateColor + "','" + qoutJG.qCamPlateNum + "','" + qoutJG.qCambiao + "','" + qoutJG.qCamPicPath + "','" + qoutJG.qJGId + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + sZuobiString + "','" + qoutRSU.qRSURandCode.ToString("X2") + "','" + "车牌匹配1" + "')";
        //                                UpdateSQLData(InsertString);
        //                                Log.WritePlateLog(DateTime.Now + " 车牌匹配成功，跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "车牌：" + qoutJG.qCamPlateNum + "入库Car表\r\n");
        //                                if (qoutJG.qCamPicPath != "未知")
        //                                {
        //                                    pictureBoxVehshow(qoutJG.qCamPicPath);//显示图片
        //                                }
        //                                if (iOBUCarType > iJGCarType)
        //                                {
        //                                    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                                }
        //                                else
        //                                {
        //                                    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                                }
        //                                plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                                updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                                listPlateNum.Remove(qoutRSU.qOBUPlateNum);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Log.WriteLog(DateTime.Now + " 一次车牌异常\r\n" + ex.ToString() + "\r\n");
        //                            }
        //                        }
        //                        else if (qoutRSU.qRSURandCode == qoutJG.qJGRandCode && qoutJG.qCamPlateNum == "未知")
        //                        {
        //                            try
        //                            {
        //                                ////标识代码匹配成功
        //                                //bTempCode = 0x0000000A;
        //                                //列入总表
        //                                InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('" + qoutJG.qJGLength + "','" + qoutJG.qJGWide + "','" + qoutJG.qJGCarType + "','" + qoutJG.qJGDateTime + "','" + qoutJG.qCamPlateColor + "','" + qoutJG.qCamPlateNum + "','" + qoutJG.qCambiao + "','" + qoutJG.qCamPicPath + "','" + qoutJG.qJGId + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + sZuobiString + "','" + qoutRSU.qRSURandCode.ToString("X2") + "','" + "位置匹配1" + "')";
        //                                UpdateSQLData(InsertString);
        //                                Log.WritePlateLog(DateTime.Now + " 跟随码匹配成功，跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "识别车牌：" + qoutJG.qCamPlateNum + "OBU车牌：" + qoutRSU.qOBUPlateNum + "入库Car表\r\n");
        //                                if (qoutJG.qCamPicPath != "未知")
        //                                {
        //                                    pictureBoxVehshow(qoutJG.qCamPicPath);//显示图片
        //                                }
        //                                if (iOBUCarType > iJGCarType)
        //                                {
        //                                    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                                }
        //                                else
        //                                {
        //                                    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                                }
        //                                updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                                listPlateNum.Remove(qoutRSU.qOBUPlateNum);
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Log.WriteLog(DateTime.Now + " 一次位置异常\r\n" + ex.ToString() + "\r\n");
        //                            }
        //                        }

        //                        else
        //                        {
        //                            try
        //                            {
        //                                //虽然队列匹配成功，但是内容不匹配
        //                                //先存OBU数据
        //                                InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + "未知" + "','" + qoutRSU.qRSURandCode.ToString("X2") + "')";
        //                                UpdateSQLData(InsertString);
        //                                Log.WritePlateLog(DateTime.Now + " 天线数据-跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库Car表\r\n");
        //                                //尝试补救匹配一下
        //                                foreach (string tempPlate in listPlateNum)
        //                                {
        //                                    if (tempPlate == qoutJG.qCamPlateNum)
        //                                    {
        //                                        if (qoutJG.qJGCarType == "未知")
        //                                        {
        //                                            sZuobiString = "作弊未知";
        //                                            iJGCarType = 0;
        //                                            iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                                        }
        //                                        else
        //                                        {
        //                                            iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                                            iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                                            if (iJGCarType <= iOBUCarType)
        //                                            {
        //                                                sZuobiString = "正常通车";
        //                                            }
        //                                            else
        //                                            {
        //                                                sZuobiString = "可能作弊";
        //                                            }
        //                                        }
        //                                        //列入总表
        //                                        InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',TradeState='" + sZuobiString + "',GetFunction='" + "车牌匹配2" + "'where OBUPlateNum='" + qoutJG.qCamPlateNum + "'";
        //                                        UpdateSQLData(InsertString);
        //                                        Log.WritePlateLog(DateTime.Now + " 激光数据补救palte成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                                        isGetbyPlate = true;
        //                                        if (qoutJG.qCamPicPath != "未知")
        //                                        {
        //                                            pictureBoxVehshow(qoutJG.qCamPicPath);//显示图片
        //                                        }
        //                                        if (iOBUCarType > iJGCarType)
        //                                        {
        //                                            plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                                        }
        //                                        else
        //                                        {
        //                                            plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                                        }
        //                                        updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                                        break;
        //                                    }
        //                                }


        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Log.WriteLog(DateTime.Now + " 位置补救异常\r\n" + ex.ToString() + "\r\n");
        //                            }
        //                            if (!isGetbyPlate)
        //                            {
        //                                try
        //                                {
        //                                    foreach (string tempRandCode in listTempCode)
        //                                    {
        //                                        if (tempRandCode == qoutJG.qJGRandCode.ToString("X2"))
        //                                        {
        //                                            if (qoutJG.qJGCarType == "未知")
        //                                            {
        //                                                sZuobiString = "作弊未知";
        //                                                iJGCarType = 0;
        //                                                iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                                            }
        //                                            else
        //                                            {
        //                                                iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                                                iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                                                if (iJGCarType <= iOBUCarType)
        //                                                {
        //                                                    sZuobiString = "正常通车";
        //                                                }
        //                                                else
        //                                                {
        //                                                    sZuobiString = "可能作弊";
        //                                                }
        //                                            }
        //                                            //列入总表
        //                                            InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',TradeState='" + sZuobiString + "',GetFunction='" + "位置匹配2" + "'where RandCode='" + tempRandCode + "'";
        //                                            UpdateSQLData(InsertString);
        //                                            Log.WritePlateLog(DateTime.Now + " 激光数据补救code成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                                            isGetbyCode = true;
        //                                            if (qoutJG.qCamPicPath != "未知")
        //                                            {
        //                                                pictureBoxVehshow(qoutJG.qCamPicPath);//显示图片
        //                                            }
        //                                            if (iOBUCarType > iJGCarType)
        //                                            {
        //                                                plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                                            }
        //                                            else
        //                                            {
        //                                                plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                                            }
        //                                            updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                                            break;
        //                                        }
        //                                    }
        //                                }
        //                                catch (Exception ex)
        //                                {
        //                                    Log.WriteLog(DateTime.Now + " 车牌补救异常\r\n" + ex.ToString() + "\r\n");
        //                                }
        //                            }
        //                            try
        //                            {
        //                                if (isGetbyPlate)
        //                                {
        //                                    if (listPlateNum.Contains(qoutRSU.qOBUPlateNum))
        //                                    {
        //                                        listPlateNum.Remove(qoutRSU.qOBUPlateNum);
        //                                    }
        //                                    isGetbyPlate = false;
        //                                }
        //                                else if (isGetbyCode)
        //                                {
        //                                    if (listTempCode.Contains(qoutRSU.qRSURandCode.ToString("X2")))
        //                                    {
        //                                        listTempCode.Remove(qoutRSU.qRSURandCode.ToString("X2"));
        //                                    }
        //                                    isGetbyCode = false;
        //                                }
        //                                else
        //                                {
        //                                    //实在是不行
        //                                    //再写JG数据
        //                                    InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + qoutJG.qJGLength + "','" + qoutJG.qJGWide + "','" + qoutJG.qJGCarType + "','" + qoutJG.qJGDateTime + "','" + qoutJG.qCamPlateColor + "','" + qoutJG.qCamPlateNum + "','" + qoutJG.qCambiao + "','" + qoutJG.qCamPicPath + "','" + qoutJG.qJGId + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "未知" + "','" + qoutJG.qJGRandCode.ToString("X2") + "')";
        //                                    UpdateSQLData(InsertString);
        //                                    Log.WritePlateLog(DateTime.Now + " 激光数据-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                                }
        //                            }
        //                            catch (Exception ex)
        //                            {
        //                                Log.WriteLog(DateTime.Now + " 一次数据匹配异常\r\n" + ex.ToString() + "\r\n");
        //                            }

        //                        }
        //                    }
        //                    else
        //                    {
        //                        //没等到
        //                        InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + "未知" + "','" + qoutRSU.qRSURandCode.ToString("X2") + "')";
        //                        UpdateSQLData(InsertString);
        //                        Log.WritePlateLog(DateTime.Now + " 天线数据1---跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库Car表\r\n");
        //                    }
        //                }
        //                else
        //                {
        //                    //没等到
        //                    InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + "未知" + "','" + qoutRSU.qRSURandCode.ToString("X2") + "')";
        //                    UpdateSQLData(InsertString);
        //                    Log.WritePlateLog(DateTime.Now + " 天线数据2---跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库Car表\r\n");
        //                    //bTempCode = qoutRSU.qRSURandCode;
        //                    //超时，一般视为激光丢车了
        //                    //仅ETC数据列入总表
        //                    //InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + "未知" + "','" + qoutRSU.qRSURandCode.ToString("X2") + "')";
        //                    //UpdateSQLData(InsertString);
        //                    //Log.WritePlateLog(DateTime.Now + " 激光数据U跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                    //plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, "未识别", "未检测", qoutRSU.qCount);
        //                }

        //            }
        //            //激光数据先到。一般是上一次没匹配上（现在有问题，由于激光的误触发，先进入此步骤，导致即使进去了也是未匹配。反而错开了顺序的匹配方式）
        //            if (qJGData.TryDequeue(out qoutJG))
        //            {
        //                isGetbyCode = false;
        //                isGetbyPlate = false;
        //                //入库
        //                InsertJGData(qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qJGCarType, qoutJG.qJGId, qoutJG.qCamPlateNum, qoutJG.qCamPicPath, qoutJG.qJGDateTime, qoutJG.qCambiao, qoutJG.qCamPlateColor, qoutJG.qJGRandCode.ToString("X2"));
        //                Log.WritePlateLog(DateTime.Now + " 激光数据O跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库JG表\r\n");

        //                //尝试补救匹配一下
        //                foreach (string tempPlate in listPlateNum)
        //                {
        //                    if (tempPlate == qoutJG.qCamPlateNum)
        //                    {
        //                        //列入总表
        //                        InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',GetFunction='" + "车牌匹配2" + "'where OBUPlateNum='" + qoutJG.qCamPlateNum + "'";
        //                        UpdateSQLData(InsertString);
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据补救palte成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                        isGetbyPlate = true;
        //                        //if (qoutJG.qJGCarType == "未知")
        //                        //{
        //                        //    iJGCarType = 0;
        //                        //    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                        //}
        //                        //else
        //                        //{
        //                        //    iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                        //    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                        //}
        //                        //if (iOBUCarType > iJGCarType)
        //                        //{
        //                        //    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                        //}
        //                        //else
        //                        //{
        //                        //    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                        //}
        //                        //updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                        break;
        //                    }
        //                }
        //                if (!isGetbyPlate)
        //                {
        //                    foreach (string tempRandCode in listTempCode)
        //                    {
        //                        if (tempRandCode == qoutJG.qJGRandCode.ToString("X2"))
        //                        {
        //                            //列入总表
        //                            InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',GetFunction='" + "位置匹配2" + "'where RandCode='" + tempRandCode + "'";
        //                            UpdateSQLData(InsertString);
        //                            Log.WritePlateLog(DateTime.Now + " 激光数据补救code成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                            isGetbyCode = true;

        //                            //if (qoutJG.qJGCarType == "未知")
        //                            //{
        //                            //    iJGCarType = 0;
        //                            //    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                            //}
        //                            //else
        //                            //{
        //                            //    iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                            //    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                            //}
        //                            //if (iOBUCarType > iJGCarType)
        //                            //{
        //                            //    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                            //}
        //                            //else
        //                            //{
        //                            //    plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                            //}
        //                            //updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                            break;
        //                        }
        //                    }
        //                }
        //                try
        //                {
        //                    if (isGetbyPlate)
        //                    {
        //                        for (int i = listPlateNum.Count - 1; i >= 0; i--)
        //                        {
        //                            if (listPlateNum[i] == qoutJG.qCamPlateNum)
        //                            {
        //                                listPlateNum.Remove(qoutJG.qCamPlateNum);
        //                            }
        //                        }
        //                        //if (listPlateNum.Contains(qoutRSU.qOBUPlateNum))
        //                        //{
        //                        //    listPlateNum.Remove(qoutRSU.qOBUPlateNum);
        //                        //}
        //                        isGetbyPlate = false;
        //                    }
        //                    else if (isGetbyCode)
        //                    {
        //                        for (int i = listTempCode.Count - 1; i >= 0; i--)
        //                        {
        //                            if (listTempCode[i] == qoutJG.qJGRandCode.ToString("X2"))
        //                            {
        //                                listTempCode.Remove(qoutJG.qJGRandCode.ToString("X2"));
        //                            }
        //                        }
        //                        //if (listTempCode.Contains(qoutRSU.qRSURandCode.ToString("X2")))
        //                        //{
        //                        //    listTempCode.Remove(qoutRSU.qRSURandCode.ToString("X2"));
        //                        //}
        //                        isGetbyCode = false;
        //                    }
        //                    else
        //                    {
        //                        //确实不匹配
        //                        //列入总表
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据补救失败-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                        InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + qoutJG.qJGLength + "','" + qoutJG.qJGWide + "','" + qoutJG.qJGCarType + "','" + qoutJG.qJGDateTime + "','" + qoutJG.qCamPlateColor + "','" + qoutJG.qCamPlateNum + "','" + qoutJG.qCambiao + "','" + qoutJG.qCamPicPath + "','" + qoutJG.qJGId + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "未知" + "','" + qoutJG.qJGRandCode.ToString("X2") + "')";
        //                        UpdateSQLData(InsertString);

        //                        //plateNoshow("未检测", "未检测", qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Log.WriteLog(DateTime.Now + " 二次数据匹配异常\r\n" + ex.ToString() + "\r\n");
        //                }
        //            }
        //            else
        //            {
        //                //都还没有，等待
        //                //rsu_inQueueDone.Reset();
        //                //if (rsu_inQueueDone.WaitOne())
        //                //{

        //                //}
        //                //居然有十多秒之后来的激光数据，我也是醉了，完全打乱我的流程
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            Log.WriteLog(DateTime.Now + " 最后数据匹配异常\r\n" + ex.ToString() + "\r\n");
        //        }
        //        Thread.Sleep(10);
        //    }
        //}
        ////第二版本的数据匹配逻辑(有问题啊)
        //public void QueueDataHanderFun2()
        //{
        //    bool isInRSUSql = false;
        //    string sZuobiString = "";
        //    int iJGCarType = 0;
        //    int iOBUCarType = 0;
        //    List<OBUList> listInfoOBU = new List<OBUList>();
        //    bool isGetbyCode = false;
        //    bool isGetbyPlate = false;
        //    string InsertString = "";

        //    while (true)
        //    {
        //        try
        //        {
        //            QueueRSUData qoutRSU = new QueueRSUData();
        //            QueueJGData qoutJG = new QueueJGData();
        //            isGetbyCode = false;
        //            isGetbyPlate = false;
        //            //先取ETC的数据
        //            if (qRSUData.TryDequeue(out qoutRSU))
        //            {
        //                //先缓存
        //                listInfoOBU.Add(new OBUList(qoutRSU.qOBUPlateNum, qoutRSU.qRSURandCode.ToString("X2"), qoutRSU.qOBUDateTime));

        //                //先进行RSU数据存储
        //                isInRSUSql = InsertRSUData(qoutRSU.qOBUPlateColor, qoutRSU.qOBUPlateNum, qoutRSU.qOBUMac, qoutRSU.qOBUY, qoutRSU.qOBUCarLength, qoutRSU.qOBUCarhigh, qoutRSU.qOBUCarType, qoutRSU.qOBUDateTime, qoutRSU.qRSURandCode.ToString("X2"));
        //                if (!isInRSUSql)
        //                {
        //                    //异常
        //                }
        //                else
        //                {
        //                    Log.WritePlateLog(DateTime.Now + " 天线数据跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库OBU表\r\n");
        //                    adddataGridViewRoll(qoutRSU.qCount, "", qoutRSU.qOBUCarType, qoutRSU.qOBUDateTime, "", qoutRSU.qOBUPlateNum, "", qoutRSU.qOBUPlateColor, "", "", "", "", "", "");
        //                }
        //                //接着存入总表
        //                InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + qoutRSU.qOBUPlateColor + "','" + qoutRSU.qOBUPlateNum + "','" + qoutRSU.qOBUMac + "','" + qoutRSU.qOBUY + "','" + qoutRSU.qOBUCarLength + "','" + qoutRSU.qOBUCarhigh + "','" + qoutRSU.qOBUCarType + "','" + qoutRSU.qOBUDateTime + "','" + "未知" + "','" + qoutRSU.qRSURandCode.ToString("X2") + "')";
        //                UpdateSQLData(InsertString);
        //                Log.WritePlateLog(DateTime.Now + " 天线数据---跟随码" + qoutRSU.qRSURandCode.ToString("X2") + "入库Car表\r\n");
        //            }
        //            if (qJGData.TryDequeue(out qoutJG))
        //            {
        //                //入库
        //                InsertJGData(qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qJGCarType, qoutJG.qJGId, qoutJG.qCamPlateNum, qoutJG.qCamPicPath, qoutJG.qJGDateTime, qoutJG.qCambiao, qoutJG.qCamPlateColor, qoutJG.qJGRandCode.ToString("X2"));
        //                Log.WritePlateLog(DateTime.Now + " 激光数据O跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库JG表\r\n");

        //                if (qoutJG.qJGCarType == "未知")
        //                {
        //                    sZuobiString = "作弊未知";
        //                    iJGCarType = 0;
        //                    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                }
        //                else
        //                {
        //                    iJGCarType = Convert.ToInt16(qoutJG.qJGCarType.Substring(1));
        //                    iOBUCarType = Convert.ToInt16(qoutRSU.qOBUCarType.Substring(1));
        //                    if (iJGCarType <= iOBUCarType)
        //                    {
        //                        sZuobiString = "正常通车";
        //                    }
        //                    else
        //                    {
        //                        sZuobiString = "可能作弊";
        //                    }

        //                }
        //                foreach (var tempOBUInfo in listInfoOBU)
        //                {

        //                    if (tempOBUInfo.sOBUPlateNumList == qoutJG.qCamPlateNum)
        //                    {
        //                        //列入总表
        //                        InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',TradeState='" + sZuobiString + "',GetFunction='" + "车牌匹配" + "'where OBUPlateNum='" + qoutJG.qCamPlateNum + "'";
        //                        UpdateSQLData(InsertString);
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据palte匹配成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                        isGetbyPlate = true;
        //                        break;
        //                    }
        //                    else if (qoutJG.qCamPlateNum == "未知" && (tempOBUInfo.sRSURandCodeList == qoutJG.qJGRandCode.ToString("X2")))
        //                    {
        //                        //列入总表
        //                        InsertString = @"Update " + sql_dbname + ".dbo.CarInfo set JGLength='" + qoutJG.qJGLength + "',JGWide='" + qoutJG.qJGWide + "',JGCarType='" + qoutJG.qJGCarType + "',ForceTime='" + qoutJG.qJGDateTime + "',CamPlateColor='" + qoutJG.qCamPlateColor + "',CamPlateNum='" + qoutJG.qCamPlateNum + "',Cambiao='" + qoutJG.qCambiao + "',CamPicPath='" + qoutJG.qCamPicPath + "',JGId='" + qoutJG.qJGId + "',TradeState='" + sZuobiString + "',GetFunction='" + "位置匹配" + "'where RandCode='" + tempOBUInfo.sRSURandCodeList + "'";
        //                        UpdateSQLData(InsertString);
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据code匹配成功-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                        isGetbyCode = true;
        //                        break;
        //                    }
        //                    else
        //                    {
        //                        //没匹配上怎么办
        //                        InsertString = @"Insert into " + sql_dbname + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode) values('" + qoutJG.qJGLength + "','" + qoutJG.qJGWide + "','" + qoutJG.qJGCarType + "','" + qoutJG.qJGDateTime + "','" + qoutJG.qCamPlateColor + "','" + qoutJG.qCamPlateNum + "','" + qoutJG.qCambiao + "','" + qoutJG.qCamPicPath + "','" + qoutJG.qJGId + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "" + "','" + "未知" + "','" + qoutJG.qJGRandCode.ToString("X2") + "')";
        //                        UpdateSQLData(InsertString);
        //                        Log.WritePlateLog(DateTime.Now + " 激光数据-跟随码" + qoutJG.qJGRandCode.ToString("X2") + "入库Car表\r\n");
        //                    }
        //                    if (isGetbyCode || isGetbyPlate)
        //                    {
        //                        isGetbyCode = false;
        //                        isGetbyPlate = false;
        //                        if (iOBUCarType > iJGCarType)
        //                        {
        //                            plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutRSU.qOBUCarType, qoutRSU.qCount);
        //                        }
        //                        else
        //                        {
        //                            plateNoshow(qoutRSU.qOBUPlateNum, qoutRSU.qOBUCarType, qoutJG.qCamPlateNum, qoutJG.qJGCarType, qoutRSU.qCount);
        //                        }
        //                        updatedataGridViewRoll(qoutRSU.qCount, qoutJG.qJGCarType, qoutJG.qJGDateTime, qoutJG.qCamPlateNum, qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qJGId, qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qCamPicPath);
        //                        //清除已匹配的列表
        //                        for (int i = listInfoOBU.Count - 1; i >= 0; i--)
        //                        {
        //                            if (listInfoOBU[i].sOBUPlateNumList == qoutJG.qCamPlateNum || listInfoOBU[i].sRSURandCodeList == qoutJG.qJGRandCode.ToString("X2"))
        //                            {
        //                                listInfoOBU.Remove(listInfoOBU[i]);
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            Log.WriteLog(DateTime.Now + " 最后数据匹配异常\r\n" + ex.ToString() + "\r\n");
        //        }
        //        Thread.Sleep(1);
        //    }
        //}
        ////RSU通信部分
        //public void RSUConnect(string s_Rsuip, string s_Rsuport)
        //{
        //    try
        //    {
        //        rsu_ip = IPAddress.Parse(s_Rsuip);
        //        rsu_server = new IPEndPoint(rsu_ip, Int32.Parse(s_Rsuport));
        //        rsu_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        rsu_sock.BeginConnect(rsu_server, new AsyncCallback(RSUConnectCallback), rsu_sock);
        //        AddOperLogCacheStr("天线正在建立连接");


        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 天线建立连接异常\r\n" + ex.ToString() + "\r\n");

        //        //MessageBox.Show(ex.ToString());
        //    }
        //}
        ////RSU连接
        //public void RSUConnectCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        Socket rsu_client = (Socket)ar.AsyncState;
        //        rsu_client.EndConnect(ar);
        //        try
        //        {
        //            RSUGetData();
        //            IsConnRSU = true;
        //        }
        //        catch
        //        {
        //            IsConnRSU = false;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 天线连接回调异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //    }

        //}
        ////RSU接收
        //public void RSUGetData()
        //{
        //    try
        //    {
        //        StateObject rsu_state = new StateObject();
        //        rsu_state.workSocket = rsu_sock;
        //        rsu_sock.BeginReceive(rsu_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RSUReceiveCallBack), rsu_state);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 天线接收数据异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //    }
        //}
        ////RSU接收回调函数
        //public void RSUReceiveCallBack(IAsyncResult ar)
        //{
        //    try
        //    {
        //        StateObject rsu_state = (StateObject)ar.AsyncState;
        //        Socket rsu_client = rsu_state.workSocket;
        //        rsu_state.revLength = rsu_client.EndReceive(ar);
        //        if (rsu_state.revLength == 0)
        //        {
        //            AddOperLogCacheStr("天线断线了");

        //            //链接断开了
        //            //rsu_client.Close();
        //            //rsu_sock.Close();
        //            IsConnRSU = false;
        //            //连接激光控制器
        //            //这里进行维护，暂未修改
        //            //RSUConnect(RSUip, RSUport);

        //        }
        //        else if (rsu_state.revLength > 0)
        //        {
        //            HeartRSUCount = 0;
        //            if(rsu_state.buffer[3]==0x9D)
        //            {
        //                TcpReply(0xD9, rsu_sock);
        //            }
        //            string ss = "";
        //            for (int i = 0; i < rsu_state.revLength; i++)
        //            {
        //                ss += rsu_state.buffer[i].ToString("X2");
        //                ss += " ";
        //            }
        //            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  收到天线数据:" + ss + "\r\n");
        //            HanderOrgData(rsu_state.buffer, rsu_state.revLength);
        //            rsu_client.BeginReceive(rsu_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RSUReceiveCallBack), rsu_state);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        IsConnRSU = false;
        //        Log.WriteLog(DateTime.Now + " 天线接收回调异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //    }
        //}

        ////激光控制器通信部分
        //public void JGConnect(string s_Jgip, string s_Jgport)
        //{
        //    try
        //    {
        //        jg_ip = IPAddress.Parse(s_Jgip);
        //        jg_server = new IPEndPoint(jg_ip, Int32.Parse(s_Jgport));
        //        jg_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //        jg_sock.BeginConnect(jg_server, new AsyncCallback(JGConnectCallback), jg_sock);
        //        AddOperLogCacheStr("激光正在建立连接");
        //        TcpReply(0x9D, jg_sock);

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 激光建立连接异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //    }
        //}
        ////JG连接
        //public void JGConnectCallback(IAsyncResult ar)
        //{
        //    try
        //    {
        //        Socket jg_client = (Socket)ar.AsyncState;
        //        jg_client.EndConnect(ar);
        //        try
        //        {

        //            JGGetData();
        //            IsConnJG = true;
        //        }
        //        catch
        //        {
        //            //IsConnJG = false;
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 激光连接回调异常\r\n" + ex.ToString() + "\r\n");
        //        //IsConnJG = false;
        //    }

        //}
        ////JG接收
        //public void JGGetData()
        //{
        //    try
        //    {
        //        StateObject jg_state = new StateObject();
        //        jg_state.workSocket = jg_sock;
        //        jg_sock.BeginReceive(jg_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(JGReceiveCallBack), jg_state);
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.WriteLog(DateTime.Now + " 激光接收数据异常\r\n" + ex.ToString() + "\r\n");
        //        //IsConnJG = false;
        //    }
        //}
        ////JG接收回调函数
        //public void JGReceiveCallBack(IAsyncResult ar)
        //{
        //    try
        //    {
        //        StateObject jg_state = (StateObject)ar.AsyncState;
        //        Socket jg_client = jg_state.workSocket;
        //        jg_state.revLength = jg_client.EndReceive(ar);
        //        if (jg_state.revLength == 0)
        //        {
        //            AddOperLogCacheStr("激光断线了");
        //            Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  激光数据长度为0"+"\r\n");
        //            //链接断开了
        //            //jg_client.Close();
        //            //jg_sock.Close();
        //            //IsConnJG = false;
        //        }
        //        else if (jg_state.revLength > 0)
        //        {
        //            HeartJGCount = 0;
        //            string ss = "";
        //            for (int i = 0; i < jg_state.revLength; i++)
        //            {
        //                ss += jg_state.buffer[i].ToString("X2");
        //                ss += " ";
        //            }
        //            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  收到激光数据:" + ss + "\r\n");
        //            HanderOrgData(jg_state.buffer, jg_state.revLength);
        //            jg_client.BeginReceive(jg_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(JGReceiveCallBack), jg_state);
        //        }

        //    }
        //    catch (Exception ex)
        //    {
        //        //IsConnJG = false;
        //        Log.WriteLog(DateTime.Now + " 激光接收回调异常\r\n" + ex.ToString() + "\r\n");
        //    }
        //}

        //#region ******数据库连接******
        //public static SqlConnection Conn
        //{
        //    get
        //    {
        //        return new SqlConnection(connStr);
        //    }
        //}
        ////执行SQL语句
        //public static SqlDataReader ExecuteQuery(string sqlStr)//(string sqlStr, params object[] param)
        //{
        //    SqlCommand cmd = new SqlCommand(sqlStr, Conn);
        //    cmd.Connection.Open();
        //    try
        //    {
        //        return cmd.ExecuteReader();
        //        //return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        //    }
        //    catch (Exception)
        //    {
        //        cmd.Connection.Close();
        //        throw;
        //    }
        //}

        //private void SQLInit()
        //{
        //    ////初始化数据库
        //    if (false == InitSqlserver())
        //    {
        //        AddOperLogCacheStr("数据库连接失败！");

        //    }
        //    else
        //    {
        //        AddOperLogCacheStr("数据库连接成功！");

        //    }
        //}

        //private bool InitSqlserver()
        //{
        //    try
        //    {
        //        if (SQLconnection == null)
        //        {
        //            string connectionString = @"Persist Security Info=True;User ID=" + sql_username + ";Password =" + sql_password + ";Initial Catalog=" + sql_dbname + ";Data Source=" + sql_ip;
        //            SQLconnection = new SqlConnection(connectionString);

        //            SQLconnection.Open();
        //        }
        //        else if (SQLconnection.State == System.Data.ConnectionState.Closed)
        //        {
        //            SQLconnection.Open();
        //        }
        //        else if (SQLconnection.State == System.Data.ConnectionState.Broken)
        //        {
        //            SQLconnection.Close();
        //            SQLconnection.Open();
        //        }

        //        //开数据库连接维护线程
        //        DataBaseConThread = new Thread(DataBaseConThr);  //数据库连接维护线程
        //        DataBaseConThread.IsBackground = true;//程序结束自动退出
        //        DataBaseConThread.Priority = ThreadPriority.BelowNormal;//Highest，AboveNormal，Normal，BelowNormal，Lowest
        //        DataBaseConThread.Start();

        //    }
        //    catch (System.Exception ex)
        //    {
        //        //MessageBox.Show(ex.ToString());
        //        Log.WriteLog(DateTime.Now + " 数据库初始化异常\r\n" + ex.ToString() + "\r\n");
        //        return false;
        //    }

        //    return true;

        //}

        //void DataBaseConThr(object statetemp)          //数据库连接维护线程
        //{
        //    while (true)
        //    {
        //        if (SQLconnection.State == System.Data.ConnectionState.Closed)
        //        {
        //            SQLconnection.Open();
        //        }
        //        else if (SQLconnection.State == System.Data.ConnectionState.Broken)
        //        {
        //            SQLconnection.Close();
        //            SQLconnection.Open();
        //            AddOperLogCacheStr("数据库重连成功！");

        //        }
        //        Thread.Sleep(7000);
        //    }

        //}
        //#endregion

        //#region ******数据库操作******
        ////插入激光数据
        //public bool InsertJGData(string s_JGCarLength, string s_JGCarWide, string s_JGCarType, string s_JGId, string s_CamPlateNum, string s_CamPicPath, string s_CamForceTime, string s_Cambiao, string s_CamPlateColor, string s_RandCode)
        //{
        //    string InsertString = @"Insert into " + sql_dbname + ".dbo.JGInfo(JGLength,JGWide,JGCarType,CamPlateNum,ForceTime,Cambiao,CamPicPath,JGId,CamPlateColor,RandCode) values('" + s_JGCarLength + "','" + s_JGCarWide + "','" + s_JGCarType + "','" + s_CamPlateNum + "','" + s_CamForceTime + "','" + s_Cambiao + "','" + s_CamPicPath + "','" + s_JGId + "','" + s_CamPlateColor + "','" + s_RandCode + "')";
        //    try
        //    {
        //        if (SQLconnection.State != System.Data.ConnectionState.Open)
        //        {
        //            AddOperLogCacheStr("激光数据插入失败！");

        //            return false;
        //        }
        //        SqlCommand cmd = new SqlCommand(InsertString, SQLconnection);
        //        cmd.ExecuteNonQuery();
        //        AddOperLogCacheStr("激光数据插入成功！");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddOperLogCacheStr("激光数据插入失败" + ex.ToString());
        //        Log.WriteLog(DateTime.Now + " 激光数据入库异常\r\n" + ex.ToString() + "\r\n");
        //        return false;
        //    }
        //}
        ////插入RSU数据
        //public bool InsertRSUData(string s_OBUPlateColor, string s_OBUPlateNum, string s_OBUMac, string s_OBUY, string s_OBUCarLength, string s_OBUCarHigh, string s_OBUCarType, string s_TradeTime, string s_RandCode)
        //{
        //    string InsertString = @"Insert into " + sql_dbname + ".dbo.OBUInfo(OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,RandCode) values('" + s_OBUPlateColor + "','" + s_OBUPlateNum + "','" + s_OBUMac + "','" + s_OBUY + "','" + s_OBUCarLength + "','" + s_OBUCarHigh + "','" + s_OBUCarType + "','" + s_TradeTime + "','" + s_RandCode + "')";
        //    try
        //    {
        //        if (SQLconnection.State != System.Data.ConnectionState.Open)
        //        {
        //            AddOperLogCacheStr("天线数据插入失败");
        //            return false;
        //        }
        //        SqlCommand cmd = new SqlCommand(InsertString, SQLconnection);
        //        cmd.ExecuteNonQuery();
        //        AddOperLogCacheStr("天线数据插入成功");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddOperLogCacheStr("天线数据插入失败" + ex.ToString());
        //        Log.WriteLog(DateTime.Now + " 天线数据入库异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //        return false;
        //    }
        //}
        ////数据更新通用函数
        //public bool UpdateSQLData(string SQLString)
        //{
        //    try
        //    {
        //        if (SQLconnection.State != System.Data.ConnectionState.Open)
        //        {
        //            AddOperLogCacheStr("数据更新失败");
        //            return false;
        //        }
        //        SqlCommand cmd = new SqlCommand(SQLString, SQLconnection);
        //        cmd.ExecuteNonQuery();
        //        AddOperLogCacheStr("数据更新成功");
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        AddOperLogCacheStr("数据更新失败" + ex.ToString());
        //        Log.WriteLog(DateTime.Now + " 数据库更新异常\r\n" + ex.ToString() + "\r\n");
        //        //MessageBox.Show(ex.ToString());
        //        return false;
        //    }
        //}
        //#endregion
    }
}
