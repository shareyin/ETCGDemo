using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using ControlExs;
using System.Threading;
using System.Runtime.InteropServices;
using System.Net;
using System.Data.SqlClient;
using System.Net.Sockets;
using ETCF.Interface;
using System.Drawing.Text;
using LayeredSkin.DirectUI;

namespace ETCF
{
    public partial class FormDemo : FormEx
    {
        #region Constructor

        public FormDemo():base()
        {
            InitializeComponent();
            initDelegateState();
        }

        #endregion

        #region Override

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                if (!DesignMode)
                {
                    cp.ExStyle |= (int)WindowStyle.WS_CLIPCHILDREN;
                }
                return cp;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            //DrawFromAlphaMainPart(this, e.Graphics);
        }

        #endregion

        #region Private

        /// <summary>
        /// 绘制窗体主体部分白色透明层
        /// </summary>
        /// <param name="form"></param>
        /// <param name="g"></param>
        public static void DrawFromAlphaMainPart(Form form, Graphics g)
        {
            Color[] colors = 
            {
                Color.FromArgb(5, Color.White),
                Color.FromArgb(30, Color.White),
                Color.FromArgb(145, Color.White),
                Color.FromArgb(150, Color.White),
                Color.FromArgb(30, Color.White),
                Color.FromArgb(5, Color.White)
            };

            float[] pos = 
            {
                0.0f,
                0.04f,
                0.10f,
                0.90f,
                0.97f,
                1.0f      
            };

            ColorBlend colorBlend = new ColorBlend(6);
            colorBlend.Colors = colors;
            colorBlend.Positions = pos;

            RectangleF destRect = new RectangleF(0, 0, form.Width, form.Height);
            using (LinearGradientBrush lBrush = new LinearGradientBrush(destRect, colors[0], colors[5], LinearGradientMode.Vertical))
            {
                lBrush.InterpolationColors = colorBlend;
                g.FillRectangle(lBrush, destRect);
            }
        }


        private void SetStyles()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            UpdateStyles();
        }

        #endregion

        string fontpath = string.Format(@"{0}\{1}", Application.StartupPath, "Digital2.ttf");
        #region******通用类******
        DataHander datah = new DataHander();
        Thread rsuProThread;//rsu维护线程
        long g_lUnixTime = 0x00000000;
        Thread jgProThread;//jg维护线程
        int index = 0;//列表计数
        int Count = 0;//接受ETC计数
        string RedicoPath = string.Format("{0}\\{1}", Application.StartupPath, "red.ico");
        string GreenicoPath = string.Format("{0}\\{1}", Application.StartupPath, "green.ico");
        System.Collections.Concurrent.ConcurrentQueue<StateObject> queue = new System.Collections.Concurrent.ConcurrentQueue<StateObject>();//用于缓存
        private static readonly object Locker1 = new object();
        private static readonly object Locker2 = new object();
        List<CamList> listCamInfo = new List<CamList>();

        Thread workThread;
        Thread queueThread;
        public StringBuilder OperLogCacheStr = new StringBuilder();//UI日志缓存
        Thread ProtectThread;
        public object UpdateOperLog_LockObj = new object();
        
        public int HeartJGCount = 0;//激光未收到数据心跳计数
        public int HeartRSUCount = 0;//天线未收到数据心跳计数
        #endregion

        #region ******数据库相关参数******
        private string sql_ip;//SQLServer的ip
        private string sql_dbname;//SQLServer数据库名称
        private string sql_username;//用户名
        private string sql_password;//密码
        private string sql_port;//端口
        private static string connStr = @"";
        public SqlConnection SQLconnection = null;
        Thread DataBaseConThread;
        #endregion

        #region ******RSU，JG，摄像机 相关参数******
        //连接天线相关
        private string RSUip;
        private string RSUport;
        private IPAddress rsu_ip;
        private IPEndPoint rsu_server;
        private Socket rsu_sock;
        public bool IsConnRSU = false;
        private static ManualResetEvent rsu_connectDone =
    new ManualResetEvent(false);
        private static ManualResetEvent rsu_inQueueDone =
            new ManualResetEvent(false);
        public byte RSCTL = 0x80;
        System.Collections.Concurrent.ConcurrentQueue<QueueRSUData> qRSUData = new System.Collections.Concurrent.ConcurrentQueue<QueueRSUData>();//用于缓存
        //连接激光相关
        private string JGip;
        private string JGport;
        private IPAddress jg_ip;
        private IPEndPoint jg_server;
        private Socket jg_sock;
        public bool IsConnJG = false;
        private static ManualResetEvent jg_connectDone =
new ManualResetEvent(false);
        private static ManualResetEvent jg_inQueueDone =
            new ManualResetEvent(false);
        System.Collections.Concurrent.ConcurrentQueue<QueueJGData> qJGData = new System.Collections.Concurrent.ConcurrentQueue<QueueJGData>();//用于缓存
        //连接摄像机
        HKCameraOutput HKOutput = new HKCameraOutput();
        public CameraClass HKcamera;
        private string HKCameraip;
        private string HKCameraUsername;
        private string HKCameraPassword;
        //摄像机相关参数
        public string GetVehicleLogoRecog = "";
        private Int32 m_lUserID = -1;
        private CHCNetSDK.MSGCallBack m_falarmData = null;
        private int iDeviceNumber = 0; //添加设备个数
        private uint iLastErr = 0;
        private string strErr;
        private Int32 m_lAlarmHandle;
        private Int32 iListenHandle = -1;
        Thread HKCameraThread;
        public string GetPlateNo = "未检测";
        public string imagepath = "未知";
        public volatile bool HKConnState = false;
        public ManualResetEvent CameraPicture = new ManualResetEvent(false);
        #endregion

        #region ******配置文件******
        //读取配置信息
        [DllImport("kernel32")]
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);
        private void readconfig()
        {
            StringBuilder temp = new StringBuilder();

            GetPrivateProfileString("SQLServer", "sql_ip", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            sql_ip = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_dbname", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            sql_dbname = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_username", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            sql_username = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_password", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            sql_password = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_port", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            sql_port = temp.ToString();

            GetPrivateProfileString("RSUconfig", "RSUIp", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            RSUip = temp.ToString();
            GetPrivateProfileString("RSUconfig", "RSUPort", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            RSUport = temp.ToString();

            GetPrivateProfileString("JGconfig", "JGIp", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            JGip = temp.ToString();
            GetPrivateProfileString("JGconfig", "JGPort", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            JGport = temp.ToString();

            GetPrivateProfileString("HKCameraconfig", "CameIP", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            HKCameraip = temp.ToString();
            GetPrivateProfileString("HKCameraconfig", "Username", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            HKCameraUsername = temp.ToString();
            GetPrivateProfileString("HKCameraconfig", "Password", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            HKCameraPassword = temp.ToString();
        }
        private void btnReadConfig_Click(object sender, EventArgs e)
        {
            StringBuilder temp = new StringBuilder();
            GetPrivateProfileString("SQLServer", "sql_ip", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbSqlIP.Text = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_dbname", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbDbName.Text = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_username", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbUserName.Text = temp.ToString();
            GetPrivateProfileString("SQLServer", "sql_password", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbPassword.Text = temp.ToString();
            //端口基本不变
            //GetPrivateProfileString("SQLServer", "sql_port", "0", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            //sql_port = temp.ToString();

            GetPrivateProfileString("RSUconfig", "RSUIp", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbRsuIP.Text = temp.ToString();
            GetPrivateProfileString("RSUconfig", "RSUPort", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbRsuPort.Text = temp.ToString();

            GetPrivateProfileString("JGconfig", "JGIp", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbJgIP.Text = temp.ToString();
            GetPrivateProfileString("JGconfig", "JGPort", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbJgPort.Text = temp.ToString();

            GetPrivateProfileString("HKCameraconfig", "CameIP", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbCamIP.Text = temp.ToString();
            GetPrivateProfileString("HKCameraconfig", "Username", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbCamName.Text = temp.ToString();
            GetPrivateProfileString("HKCameraconfig", "Password", "异常", temp, 255, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            tbCamPassword.Text = temp.ToString();
        }
        [DllImport("kernel32")]
        public static extern long WritePrivateProfileString(string section, string key,
            string val, string filePath);
        private void btnSaveConfig_Click(object sender, EventArgs e)
        {
            WritePrivateProfileString("SQLServer", "sql_ip", tbSqlIP.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("SQLServer", "sql_dbname", tbDbName.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("SQLServer", "sql_username", tbUserName.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("SQLServer", "sql_password", tbPassword.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");

            WritePrivateProfileString("RSUconfig", "RSUIp", tbRsuIP.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("RSUconfig", "RSUPort", tbRsuPort.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");

            WritePrivateProfileString("JGconfig", "JGIp", tbJgIP.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("JGconfig", "JGPort", tbJgPort.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");

            WritePrivateProfileString("HKCameraconfig", "sql_ip", tbCamIP.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("HKCameraconfig", "sql_ip", tbCamName.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
            WritePrivateProfileString("HKCameraconfig", "sql_ip", tbCamPassword.Text, AppDomain.CurrentDomain.SetupInformation.ApplicationBase + "config.ini");
        }
        #endregion
        //窗口启动，加载配置文件，连接数据库，摄像机，天线，激光器
        private void FormDemo_Load(object sender, EventArgs e)
        {
            this.Left = (Screen.PrimaryScreen.WorkingArea.Width - Width) / 2;
            this.Top = (Screen.PrimaryScreen.WorkingArea.Height - Height) / 2;
            new Thread(() => { UpdateOperLogThread(); }).Start();

            PrivateFontCollection pfc = new PrivateFontCollection();
            pfc.AddFontFile(fontpath);
            Font Numfont = new Font(pfc.Families[0], 20);
            //labelNum.Font = Numfont;
            try
            {
                //读取配置文件
                readconfig();

            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 配置文件读取异常\r\n" + ex.ToString() + "\r\n");
                return;
            }
            try
            {
                //连接天线控制器
                RSUConnect(RSUip, RSUport);
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 连接天线异常\r\n" + ex.ToString() + "\r\n");
            }
            try
            {
                //连接激光控制器
                JGConnect(JGip, JGport);
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 连接激光异常\r\n" + ex.ToString() + "\r\n");
                MessageBox.Show(ex.ToString());
            }
            try
            {
                //摄像机连接
                initHK();
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 连接摄像机异常\r\n" + ex.ToString() + "\r\n");
            }
            try
            {
                //数据库连接
                connStr = @"Server=" + sql_ip + ";uid=" + sql_username + ";pwd=" + sql_password + ";database=" + sql_dbname;
                SQLInit();
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 数据库初始化异常\r\n" + ex.ToString() + "\r\n");
            }
            ProtectPro();

            //数据解析线程
            ResThread();
            //数据入库
            QueueHanderThread();
            //定时心跳
            timer1.Start();
        }
        #region******线程维护部分******
        //维护线程
        private void ProtectPro()
        {
            ProtectThread = new Thread(ProtectBase);
            ProtectThread.IsBackground = true;
            ProtectThread.Priority = ThreadPriority.BelowNormal;
            ProtectThread.Start();
        }
        public void ProtectBase(object statetemp)
        {
            while (true)
            {
                //摄像头重连
                if (HKConnState == false)
                {
                    pictureBoxCam.BackgroundImage = Image.FromFile(@RedicoPath);
                    initHK();
                    Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 摄像机已检测断开，正在重连\r\n");
                }
                else
                {
                    pictureBoxCam.BackgroundImage = Image.FromFile(@GreenicoPath);
                }
                //RSU重连
                if (!IsConnRSU)
                {
                    pictureBoxRSU.BackgroundImage = Image.FromFile(@RedicoPath);
                    RSUConnect(RSUip, RSUport);
                    Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 天线控制器已检测断开，正在重连\r\n");
                }
                else
                {
                    pictureBoxRSU.BackgroundImage = Image.FromFile(@GreenicoPath);
                }
                //机关重连
                if (!IsConnJG)
                {
                    pictureBoxJG.BackgroundImage = Image.FromFile(@RedicoPath);
                    JGConnect(JGip, JGport);
                    Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 激光控制器已检测断开，正在重连\r\n");
                }
                else
                {
                    pictureBoxJG.BackgroundImage = Image.FromFile(@GreenicoPath);
                }
                Thread.Sleep(3000);
            }
        }
        #endregion

        #region******UI委托部分******
        //UI委托类初始化函数
        private void initDelegateState()
        {
            //AddOperLogCacheStr = controllogtext;

            DelegateState.pictureBoxVehshow = pictureBoxVehshow;
            DelegateState.plateNoshow = plateNoshow;
            DelegateState.adddataGridViewRoll = adddataGridViewRoll;
            DelegateState.updatedataGridViewRoll = updatedataGridViewRoll;

            DelegateState.InsertGridview = InsertGridview;
        }
        //添加表格文本
        private void adddataGridViewRoll(string s_Id, string s_JgCarType, string s_RsuCarType, string s_RsuTradeTime, string s_JgTime, string s_RsuPlateNum, string s_CamPlateNum, string s_RsuPlateColor, string s_CamPlateColor, string s_Cambiao, string s_JgId, string s_JgLength, string s_JgWide, string s_CamPicPath)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    int i = 0;
                    if (index < 60)
                        index = this.dataGridViewRoll.Rows.Add();
                    //整体移动
                    if (index != 0)//多条记录
                    {
                        if (index > 80)
                        {
                            index = 80;
                        }

                        for (i = index; i > 0; i--)
                        {
                            //序号
                            this.dataGridViewRoll.Rows[i].Cells[0].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[0].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[0].Style.ForeColor = Color.Black;
                            //检测车型
                            this.dataGridViewRoll.Rows[i].Cells[1].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[1].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[1].Style.ForeColor = Color.Green;
                            //OBU车型
                            this.dataGridViewRoll.Rows[i].Cells[2].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[2].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[2].Style.ForeColor = Color.Green;
                            //交易时间
                            this.dataGridViewRoll.Rows[i].Cells[3].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[3].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[3].Style.ForeColor = Color.Black;
                            //抓拍时间
                            this.dataGridViewRoll.Rows[i].Cells[4].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[4].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[4].Style.ForeColor = Color.Black;
                            //OBU车牌
                            this.dataGridViewRoll.Rows[i].Cells[5].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[5].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[5].Style.ForeColor = Color.Green;
                            //检测车牌
                            this.dataGridViewRoll.Rows[i].Cells[6].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[6].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[6].Style.ForeColor = Color.Green;
                            //OBU车牌颜色
                            this.dataGridViewRoll.Rows[i].Cells[7].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[7].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[7].Style.ForeColor = Color.Black;
                            //检测车牌颜色
                            this.dataGridViewRoll.Rows[i].Cells[8].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[8].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[8].Style.ForeColor = Color.Black;
                            //识别车标
                            this.dataGridViewRoll.Rows[i].Cells[9].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[9].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[9].Style.ForeColor = Color.Black;
                            //激光序号
                            this.dataGridViewRoll.Rows[i].Cells[10].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[10].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[10].Style.ForeColor = Color.Black;
                            //车长
                            this.dataGridViewRoll.Rows[i].Cells[11].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[11].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[11].Style.ForeColor = Color.Black;
                            //车宽
                            this.dataGridViewRoll.Rows[i].Cells[12].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[12].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[12].Style.ForeColor = Color.Black;
                            //图片路劲
                            this.dataGridViewRoll.Rows[i].Cells[13].Value = (this.dataGridViewRoll.Rows[i - 1].Cells[13].Value).ToString();
                            this.dataGridViewRoll.Rows[i].Cells[13].Style.ForeColor = Color.Black;
                        }
                    }

                    //序号
                    this.dataGridViewRoll.Rows[0].Cells[0].Value = s_Id;
                    this.dataGridViewRoll.Rows[0].Cells[0].Style.ForeColor = Color.Black;
                    //检测车型
                    this.dataGridViewRoll.Rows[0].Cells[1].Value = s_JgCarType;
                    this.dataGridViewRoll.Rows[0].Cells[1].Style.ForeColor = Color.Green;
                    //Obu车型
                    this.dataGridViewRoll.Rows[0].Cells[2].Value = s_RsuCarType;
                    this.dataGridViewRoll.Rows[0].Cells[2].Style.ForeColor = Color.Green;
                    //交易时间
                    this.dataGridViewRoll.Rows[0].Cells[3].Value = s_RsuTradeTime;
                    this.dataGridViewRoll.Rows[0].Cells[3].Style.ForeColor = Color.Black;
                    //抓拍时间
                    this.dataGridViewRoll.Rows[0].Cells[4].Value = s_JgTime;
                    this.dataGridViewRoll.Rows[0].Cells[4].Style.ForeColor = Color.Black;
                    //OBU车牌
                    this.dataGridViewRoll.Rows[0].Cells[5].Value = s_RsuPlateNum;
                    this.dataGridViewRoll.Rows[0].Cells[5].Style.ForeColor = Color.Green;
                    //识别车牌
                    this.dataGridViewRoll.Rows[0].Cells[6].Value = s_CamPlateNum;
                    this.dataGridViewRoll.Rows[0].Cells[6].Style.ForeColor = Color.Green;
                    //OBU车牌颜色
                    this.dataGridViewRoll.Rows[0].Cells[7].Value = s_RsuPlateColor;
                    this.dataGridViewRoll.Rows[0].Cells[7].Style.ForeColor = Color.Black;
                    //识别车牌颜色
                    this.dataGridViewRoll.Rows[0].Cells[8].Value = s_CamPlateColor;
                    this.dataGridViewRoll.Rows[0].Cells[8].Style.ForeColor = Color.Black;
                    //识别车标
                    this.dataGridViewRoll.Rows[0].Cells[9].Value = s_Cambiao;
                    this.dataGridViewRoll.Rows[0].Cells[9].Style.ForeColor = Color.Black;
                    //激光序号
                    this.dataGridViewRoll.Rows[0].Cells[10].Value = s_JgId;
                    this.dataGridViewRoll.Rows[0].Cells[10].Style.ForeColor = Color.Black;
                    //车长
                    this.dataGridViewRoll.Rows[0].Cells[11].Value = s_JgLength;
                    this.dataGridViewRoll.Rows[0].Cells[11].Style.ForeColor = Color.Black;
                    //车宽
                    this.dataGridViewRoll.Rows[0].Cells[12].Value = s_JgWide;
                    this.dataGridViewRoll.Rows[0].Cells[12].Style.ForeColor = Color.Black;
                    //图片路劲
                    this.dataGridViewRoll.Rows[0].Cells[13].Value = s_CamPicPath;
                    this.dataGridViewRoll.Rows[0].Cells[13].Style.ForeColor = Color.Black;

                    this.dataGridViewRoll.FirstDisplayedScrollingRowIndex = 0;//显示最新一行

                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }
        //更新表格文本
        private void updatedataGridViewRoll(string s_Id, string s_JgCarType, string s_JgTime, string s_CamPlateNum, string s_CamPlateColor, string s_Cambiao, string s_JgId, string s_JgLength, string s_JgWide, string s_CamPicPath)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    int i = 0;
                    for (i = 0; i < 4; i++)
                    {
                        if (s_CamPlateNum == this.dataGridViewRoll.Rows[i].Cells[5].Value.ToString())
                        {
                            //检测车型
                            this.dataGridViewRoll.Rows[0].Cells[1].Value = s_JgCarType;
                            this.dataGridViewRoll.Rows[0].Cells[1].Style.ForeColor = Color.Green;
                            //抓拍时间
                            this.dataGridViewRoll.Rows[0].Cells[4].Value = s_JgTime;
                            this.dataGridViewRoll.Rows[0].Cells[4].Style.ForeColor = Color.Black;
                            //识别车牌
                            this.dataGridViewRoll.Rows[0].Cells[6].Value = s_CamPlateNum;
                            this.dataGridViewRoll.Rows[0].Cells[6].Style.ForeColor = Color.Green;
                            //识别车牌颜色
                            this.dataGridViewRoll.Rows[0].Cells[8].Value = s_CamPlateColor;
                            this.dataGridViewRoll.Rows[0].Cells[8].Style.ForeColor = Color.Black;
                            //识别车标
                            this.dataGridViewRoll.Rows[0].Cells[9].Value = s_Cambiao;
                            this.dataGridViewRoll.Rows[0].Cells[9].Style.ForeColor = Color.Black;
                            //激光序号
                            this.dataGridViewRoll.Rows[0].Cells[10].Value = s_JgId;
                            this.dataGridViewRoll.Rows[0].Cells[10].Style.ForeColor = Color.Black;
                            //车长
                            this.dataGridViewRoll.Rows[0].Cells[11].Value = s_JgLength;
                            this.dataGridViewRoll.Rows[0].Cells[11].Style.ForeColor = Color.Black;
                            //车宽
                            this.dataGridViewRoll.Rows[0].Cells[12].Value = s_JgWide;
                            this.dataGridViewRoll.Rows[0].Cells[12].Style.ForeColor = Color.Black;
                            //图片路劲
                            this.dataGridViewRoll.Rows[0].Cells[13].Value = s_CamPicPath;
                            this.dataGridViewRoll.Rows[0].Cells[13].Style.ForeColor = Color.Black;
                            break;
                        }
                    }
                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }


        }
        //图片显示
        private void pictureBoxVehshow(string msg)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    this.pictureBoxVeh.Image = null;//待测
                    this.pictureBoxVeh.Load(msg);
                    this.pictureBoxVeh.SizeMode = PictureBoxSizeMode.StretchImage;
                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        //提示框显示（耗时太长，已取消）
        private void controllogtext(string msg)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    this.controltext.AppendText(DateTime.Now + ":" + msg + Environment.NewLine);
                    
                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        /********************新添加*****************/
        //定时更新日志线程
        public void UpdateOperLogThread()
        {
            while (true)
            {
                Thread.Sleep(GlobalMember.OperLogFreshTime);
                lock (UpdateOperLog_LockObj)
                {
                    if ("".Equals(OperLogCacheStr.ToString()))
                    {
                        continue;
                    }
                    else
                    {
                        WriteOperLog(OperLogCacheStr.ToString());
                        if (GlobalMember.WriteLogSwitch)
                        {
                            Log.WriteLog(OperLogCacheStr.ToString());
                        }
                        OperLogCacheStr.Clear();
                    }
                }
            }
        }
        //定义委托
        public delegate void MyDelegate_WriteOperLogDeleFun(string log);

        public void WriteOperLog(string log)
        {
            if (this.InvokeRequired)
            {
                MyDelegate_WriteOperLogDeleFun md = new MyDelegate_WriteOperLogDeleFun(WriteOperLogDeleFun);
                this.BeginInvoke(md, log);
            }
            else
            {
                WriteOperLogDeleFun(log);
            }
        }
        //更新UI显示日志
        public void WriteOperLogDeleFun(string log)
        {
            if (this.controltext.Text.Length > 102400)
            {
                this.controltext.Text = log;
            }
            else
            {
                this.controltext.AppendText(log);
            }
        }
        //向UI日志缓存里面添加内容
        public void AddOperLogCacheStr(string log)
        {
            lock (UpdateOperLog_LockObj)
            {
                OperLogCacheStr.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "\r\n" + log + "\r\n\r\n");
            }
        }
        /********************新添加完事*****************/
        //前端界面显示车牌车型等信息
        private void plateNoshow(string s_OBUPlateNum, string s_OBUCarType, string s_CamPlateNum, string s_JGCarType, string s_Num)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    //OBU车牌号
                    this.labelOBUPlateNum.Text = s_OBUPlateNum;
                    this.labelOBUPlateNum.Text += "    ";

                    //OBU车型
                    this.labelOBUCarType.Text = s_OBUCarType;
                    this.labelOBUCarType.Text += "    ";
                    //识别车牌                    
                    this.labelCamPlateNum.Text = s_CamPlateNum;
                    this.labelCamPlateNum.Text += "    ";

                    //激光车型
                    this.labelJGCarType.Text = s_JGCarType;
                    this.labelJGCarType.Text += "    ";

                    //过车总数
                    this.labelNum.Text = s_Num;
                    this.labelNum.Text += "    ";
                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        //显示到表格上
        private void InsertGridview(string s_Id, string s_RsuTradeTime, string s_RsuPlateNum, string s_RsuCarType, string s_JgCarType, string s_IsZuobi, string s_JgLength, string s_JgWide, string s_CamPicPath)
        {
            try
            {
                this.Invoke(new ThreadStart(delegate
                {
                    int index = this.dataGridView1.Rows.Add();

                    //序号
                    this.dataGridView1.Rows[index].Cells[0].Value = s_Id;
                    //交易时间
                    this.dataGridView1.Rows[index].Cells[1].Value = s_RsuTradeTime;
                    //OBU车牌
                    this.dataGridView1.Rows[index].Cells[2].Value = s_RsuPlateNum;
                    this.dataGridView1.Rows[index].Cells[2].Style.ForeColor = Color.Blue;
                    //OBU车型
                    this.dataGridView1.Rows[index].Cells[3].Value = s_RsuCarType;
                    this.dataGridView1.Rows[index].Cells[3].Style.ForeColor = Color.Blue;
                    //激光车型
                    this.dataGridView1.Rows[index].Cells[4].Value = s_JgCarType;
                    this.dataGridView1.Rows[index].Cells[4].Style.ForeColor = Color.Blue;
                    //可能作弊
                    this.dataGridView1.Rows[index].Cells[5].Value = s_IsZuobi;
                    this.dataGridView1.Rows[index].Cells[5].Style.ForeColor = Color.Red;
                    //车长
                    this.dataGridView1.Rows[index].Cells[6].Value = s_JgLength;
                    this.dataGridView1.Rows[index].Cells[6].Style.ForeColor = Color.Black;
                    //车高
                    this.dataGridView1.Rows[index].Cells[7].Value = s_JgWide;
                    this.dataGridView1.Rows[index].Cells[7].Style.ForeColor = Color.Black;
                    //图片路径
                    this.dataGridView1.Rows[index].Cells[8].Value = s_CamPicPath;//路径

                    //this.dataGridView1.FirstDisplayedScrollingRowIndex = this.dataGridView1.RowCount - 1;//06-06,显示最新一行

                }));
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        #endregion

        #region    ******RSU建立连接******
        //RSU通信部分
        public void RSUConnect(string s_Rsuip, string s_Rsuport)
        {
            try
            {
                rsu_ip = IPAddress.Parse(s_Rsuip);
                rsu_server = new IPEndPoint(rsu_ip, Int32.Parse(s_Rsuport));
                rsu_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                rsu_sock.BeginConnect(rsu_server, new AsyncCallback(RSUConnectCallback), rsu_sock);
                AddOperLogCacheStr("天线正在建立连接");
                

            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 天线建立连接异常\r\n" + ex.ToString() + "\r\n");

                //MessageBox.Show(ex.ToString());
            }
        }
        //RSU连接
        public void RSUConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket rsu_client = (Socket)ar.AsyncState;
                rsu_client.EndConnect(ar);
                try
                {
                    RSUGetData();
                    IsConnRSU = true;
                }
                catch
                {
                    IsConnRSU = false;
                }
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 天线连接回调异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
            }

        }
        //RSU接收
        public void RSUGetData()
        {
            try
            {
                StateObject rsu_state = new StateObject();
                rsu_state.workSocket = rsu_sock;
                rsu_sock.BeginReceive(rsu_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RSUReceiveCallBack), rsu_state);
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 天线接收数据异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
            }
        }
        //RSU接收回调函数
        public void RSUReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                StateObject rsu_state = (StateObject)ar.AsyncState;
                Socket rsu_client = rsu_state.workSocket;
                rsu_state.revLength = rsu_client.EndReceive(ar);
                if (rsu_state.revLength == 0)
                {
                    AddOperLogCacheStr("天线断线了");
                    
                    //链接断开了
                    //rsu_client.Close();
                    //rsu_sock.Close();
                    IsConnRSU = false;
                    //连接激光控制器
                    //这里进行维护，暂未修改
                    //RSUConnect(RSUip, RSUport);

                }
                else if (rsu_state.revLength > 0)
                {
                    HeartRSUCount = 0;
                    if(rsu_state.buffer[3]==0x9D)
                    {
                        TcpReply(0xD9, rsu_sock);
                    }
                    string ss = "";
                    for (int i = 0; i < rsu_state.revLength; i++)
                    {
                        ss += rsu_state.buffer[i].ToString("X2");
                        ss += " ";
                    }
                    Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  收到天线数据:" + ss + "\r\n");
                    HanderOrgData(rsu_state.buffer, rsu_state.revLength);
                    rsu_client.BeginReceive(rsu_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(RSUReceiveCallBack), rsu_state);
                }

            }
            catch (Exception ex)
            {
                IsConnRSU = false;
                Log.WriteLog(DateTime.Now + " 天线接收回调异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
            }
        }
        #endregion

        #region    ******JG建立连接******
        //激光控制器通信部分
        public void JGConnect(string s_Jgip, string s_Jgport)
        {
            try
            {
                jg_ip = IPAddress.Parse(s_Jgip);
                jg_server = new IPEndPoint(jg_ip, Int32.Parse(s_Jgport));
                jg_sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                jg_sock.BeginConnect(jg_server, new AsyncCallback(JGConnectCallback), jg_sock);
                AddOperLogCacheStr("激光正在建立连接");
                TcpReply(0x9D, jg_sock);

            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 激光建立连接异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
            }
        }
        //JG连接
        public void JGConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket jg_client = (Socket)ar.AsyncState;
                jg_client.EndConnect(ar);
                try
                {
                    
                    JGGetData();
                    IsConnJG = true;
                }
                catch
                {
                    //IsConnJG = false;
                }

            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 激光连接回调异常\r\n" + ex.ToString() + "\r\n");
                //IsConnJG = false;
            }

        }
        //JG接收
        public void JGGetData()
        {
            try
            {
                StateObject jg_state = new StateObject();
                jg_state.workSocket = jg_sock;
                jg_sock.BeginReceive(jg_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(JGReceiveCallBack), jg_state);
            }
            catch (Exception ex)
            {
                Log.WriteLog(DateTime.Now + " 激光接收数据异常\r\n" + ex.ToString() + "\r\n");
                //IsConnJG = false;
            }
        }
        //JG接收回调函数
        public void JGReceiveCallBack(IAsyncResult ar)
        {
            try
            {
                StateObject jg_state = (StateObject)ar.AsyncState;
                Socket jg_client = jg_state.workSocket;
                jg_state.revLength = jg_client.EndReceive(ar);
                if (jg_state.revLength == 0)
                {
                    AddOperLogCacheStr("激光断线了");
                    Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  激光数据长度为0"+"\r\n");
                    //链接断开了
                    //jg_client.Close();
                    //jg_sock.Close();
                    //IsConnJG = false;
                }
                else if (jg_state.revLength > 0)
                {
                    HeartJGCount = 0;
                    string ss = "";
                    for (int i = 0; i < jg_state.revLength; i++)
                    {
                        ss += jg_state.buffer[i].ToString("X2");
                        ss += " ";
                    }
                    Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  收到激光数据:" + ss + "\r\n");
                    HanderOrgData(jg_state.buffer, jg_state.revLength);
                    jg_client.BeginReceive(jg_state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(JGReceiveCallBack), jg_state);
                }

            }
            catch (Exception ex)
            {
                //IsConnJG = false;
                Log.WriteLog(DateTime.Now + " 激光接收回调异常\r\n" + ex.ToString() + "\r\n");
            }
        }
        #endregion

        #region ******数据库连接******
        public static SqlConnection Conn
        {
            get
            {
                return new SqlConnection(connStr);
            }
        }
        //执行SQL语句
        public static SqlDataReader ExecuteQuery(string sqlStr)//(string sqlStr, params object[] param)
        {
            SqlCommand cmd = new SqlCommand(sqlStr, Conn);
            cmd.Connection.Open();
            try
            {
                return cmd.ExecuteReader();
                //return cmd.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
            }
            catch (Exception)
            {
                cmd.Connection.Close();
                throw;
            }
        }

        private void SQLInit()
        {
            ////初始化数据库
            if (false == InitSqlserver())
            {
                AddOperLogCacheStr("数据库连接失败！");
                
            }
            else
            {
                AddOperLogCacheStr("数据库连接成功！");
                
            }
        }

        private bool InitSqlserver()
        {
            try
            {
                if (SQLconnection == null)
                {
                    string connectionString = @"Persist Security Info=True;User ID=" + sql_username + ";Password =" + sql_password + ";Initial Catalog=" + sql_dbname + ";Data Source=" + sql_ip;
                    SQLconnection = new SqlConnection(connectionString);

                    SQLconnection.Open();
                }
                else if (SQLconnection.State == System.Data.ConnectionState.Closed)
                {
                    SQLconnection.Open();
                }
                else if (SQLconnection.State == System.Data.ConnectionState.Broken)
                {
                    SQLconnection.Close();
                    SQLconnection.Open();
                }

                //开数据库连接维护线程
                DataBaseConThread = new Thread(DataBaseConThr);  //数据库连接维护线程
                DataBaseConThread.IsBackground = true;//程序结束自动退出
                DataBaseConThread.Priority = ThreadPriority.BelowNormal;//Highest，AboveNormal，Normal，BelowNormal，Lowest
                DataBaseConThread.Start();

            }
            catch (System.Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                Log.WriteLog(DateTime.Now + " 数据库初始化异常\r\n" + ex.ToString() + "\r\n");
                return false;
            }

            return true;

        }
        void DataBaseConThr(object statetemp)          //数据库连接维护线程
        {
            while (true)
            {
                if (SQLconnection.State == System.Data.ConnectionState.Closed)
                {
                    SQLconnection.Open();
                }
                else if (SQLconnection.State == System.Data.ConnectionState.Broken)
                {
                    SQLconnection.Close();
                    SQLconnection.Open();
                    AddOperLogCacheStr("数据库重连成功！");
                    
                }
                Thread.Sleep(7000);
            }

        }
        #endregion

        #region ******数据库操作******
        //插入激光数据
        public bool InsertJGData(string s_JGCarLength, string s_JGCarWide, string s_JGCarType, string s_JGId, string s_CamPlateNum, string s_CamPicPath, string s_CamForceTime, string s_Cambiao, string s_CamPlateColor, string s_RandCode)
        {
            string InsertString = @"Insert into " + sql_dbname + ".dbo.JGInfo(JGLength,JGWide,JGCarType,CamPlateNum,ForceTime,Cambiao,CamPicPath,JGId,CamPlateColor,RandCode) values('" + s_JGCarLength + "','" + s_JGCarWide + "','" + s_JGCarType + "','" + s_CamPlateNum + "','" + s_CamForceTime + "','" + s_Cambiao + "','" + s_CamPicPath + "','" + s_JGId + "','" + s_CamPlateColor + "','" + s_RandCode + "')";
            try
            {
                if (SQLconnection.State != System.Data.ConnectionState.Open)
                {
                    AddOperLogCacheStr("激光数据插入失败！");
                    
                    return false;
                }
                SqlCommand cmd = new SqlCommand(InsertString, SQLconnection);
                cmd.ExecuteNonQuery();
                AddOperLogCacheStr("激光数据插入成功！");
                return true;
            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("激光数据插入失败" + ex.ToString());
                Log.WriteLog(DateTime.Now + " 激光数据入库异常\r\n" + ex.ToString() + "\r\n");
                return false;
            }
        }
        //插入RSU数据
        public bool InsertRSUData(string s_OBUPlateColor, string s_OBUPlateNum, string s_OBUMac, string s_OBUY, string s_OBUCarLength, string s_OBUCarHigh, string s_OBUCarType, string s_TradeTime, string s_RandCode)
        {
            string InsertString = @"Insert into " + sql_dbname + ".dbo.OBUInfo(OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,RandCode) values('" + s_OBUPlateColor + "','" + s_OBUPlateNum + "','" + s_OBUMac + "','" + s_OBUY + "','" + s_OBUCarLength + "','" + s_OBUCarHigh + "','" + s_OBUCarType + "','" + s_TradeTime + "','" + s_RandCode + "')";
            try
            {
                if (SQLconnection.State != System.Data.ConnectionState.Open)
                {
                    AddOperLogCacheStr("天线数据插入失败");
                    return false;
                }
                SqlCommand cmd = new SqlCommand(InsertString, SQLconnection);
                cmd.ExecuteNonQuery();
                AddOperLogCacheStr("天线数据插入成功");
                return true;
            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("天线数据插入失败" + ex.ToString());
                Log.WriteLog(DateTime.Now + " 天线数据入库异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
                return false;
            }
        }
        //数据更新通用函数
        public bool UpdateSQLData(string SQLString)
        {
            try
            {
                if (SQLconnection.State != System.Data.ConnectionState.Open)
                {
                    AddOperLogCacheStr("数据更新失败");
                    return false;
                }
                SqlCommand cmd = new SqlCommand(SQLString, SQLconnection);
                cmd.ExecuteNonQuery();
                AddOperLogCacheStr("数据更新成功");
                return true;
            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("数据更新失败" + ex.ToString());
                Log.WriteLog(DateTime.Now + " 数据库更新异常\r\n" + ex.ToString() + "\r\n");
                //MessageBox.Show(ex.ToString());
                return false;
            }
        }
        #endregion

        #region ******摄像机处理流程******
        //摄像头初始化
        private void initHK()
        {
            int res = 0;
            bool m_bInitSDK = CHCNetSDK.NET_DVR_Init();
            if (m_bInitSDK == true)
            {
                m_falarmData = new CHCNetSDK.MSGCallBack(MsgCallback);
                bool btemp = CHCNetSDK.NET_DVR_SetDVRMessageCallBack_V30(m_falarmData, IntPtr.Zero);
                if (btemp != true)
                {
                    AddOperLogCacheStr("SetDVRMessageCallBack_V30返回失败");
                    return;
                }
                res = camera_Login();
                if (res != 0)
                {
                    AddOperLogCacheStr("摄像头登录失败");
                    return;
                }
                res = camera_SetAlarm();
                if (res != 0)
                {
                    AddOperLogCacheStr("摄像头布防失败");
                    return;
                }
                res = camera_StartListen();
                if (res != 0)
                {
                    AddOperLogCacheStr("摄像头启动监听失败");
                    return;
                }
                HKConnState = true;
                AddOperLogCacheStr("Initialize返回0,摄像头连接成功！");
            }
            else
            {
                AddOperLogCacheStr("Initialize返回-1,摄像头连接失败！");
            }
        }

        private int camera_Login()
        {
            string DVRIPAddress = HKCameraip;//设备IP地址或者域名 Device IP
            Int16 DVRPortNumber = Int16.Parse("8000");//设备服务端口号 Device Port
            string DVRUserName = HKCameraUsername;//设备登录用户名 User name to login
            string DVRPassword = HKCameraPassword;//设备登录密码 Password to login
            CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V30();

            //登录设备 Login the device
            m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);
            if (m_lUserID < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                strErr = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号 Failed to login and output the error code
                Log.WriteLog(DateTime.Now + " 摄像机登录失败\r\n" + iLastErr + "\r\n");
                //MessageBox.Show(strErr);
                return -3;
            }
            else
            {
                //登录成功
                iDeviceNumber++;
                string str1 = "" + m_lUserID;
                //listViewDevice.Items.Add(new ListViewItem(new string[] { str1, DVRIPAddress, "未布防" }));//将已注册设备添加进列表
                return 0;
            }
        }
        private int camera_SetAlarm()
        {
            CHCNetSDK.NET_DVR_SETUPALARM_PARAM struAlarmParam = new CHCNetSDK.NET_DVR_SETUPALARM_PARAM();
            struAlarmParam.dwSize = (uint)Marshal.SizeOf(struAlarmParam);
            struAlarmParam.byLevel = 1; //0- 一级布防,1- 二级布防
            struAlarmParam.byAlarmInfoType = 1;//智能交通设备有效，新报警信息类型
            struAlarmParam.byFaceAlarmDetection = 1;//1-人脸侦测

            for (int i = 0; i < iDeviceNumber; i++)
            {

                m_lAlarmHandle = CHCNetSDK.NET_DVR_SetupAlarmChan_V41(m_lUserID, ref struAlarmParam);
                if (m_lAlarmHandle < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    strErr = "布防失败，错误号：" + iLastErr; //布防失败，输出错误号
                    //MessageBox.Show(strErr);
                    return -1;
                }

            }
            return 0;
        }

        private int camera_CloseAlarm()
        {
            for (int i = 0; i < iDeviceNumber; i++)
            {

                if (m_lAlarmHandle >= 0)
                {
                    if (!CHCNetSDK.NET_DVR_CloseAlarmChan_V30(m_lAlarmHandle))
                    {
                        iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                        strErr = "撤防失败，错误号：" + iLastErr; //撤防失败，输出错误号                      
                        //MessageBox.Show(strErr);
                        return -1;
                    }

                }
                else
                {
                    strErr = "未布防";
                    //MessageBox.Show(strErr);
                }
            }
            return 0;
        }

        private int camera_Exit()
        {
            int res = 0;
            //撤防
            res = camera_CloseAlarm();
            if (res != 0) return res;

            //停止监听
            if (iListenHandle >= 0)
            {
                CHCNetSDK.NET_DVR_StopListen_V30(iListenHandle);
            }

            //注销登录
            for (int i = 0; i < iDeviceNumber; i++)
            {

                CHCNetSDK.NET_DVR_Logout(m_lUserID);
            }

            //释放SDK资源，在程序结束之前调用
            CHCNetSDK.NET_DVR_Cleanup();
            return 0;

        }

        private int camera_StartListen()
        {
            byte[] strIP = new byte[16 * 16];
            uint dwValidNum = 0;
            Boolean bEnableBind = false;
            string sLocalIP = "";
            string sLocalPort = "7200";
            ushort wLocalPort = ushort.Parse(sLocalPort);
            int res = 0;
            //获取本地PC网卡IP信息
            if (CHCNetSDK.NET_DVR_GetLocalIP(strIP, ref dwValidNum, ref bEnableBind))
            {
                if (dwValidNum > 0)
                {
                    //取第一张网卡的IP地址为默认监听端口
                    sLocalIP = System.Text.Encoding.UTF8.GetString(strIP, 0, 16);
                }

            }
            iListenHandle = CHCNetSDK.NET_DVR_StartListen_V30(sLocalIP, wLocalPort, m_falarmData, IntPtr.Zero);
            if (iListenHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                strErr = "启动监听失败，错误号：" + iLastErr; //撤防失败，输出错误号
                //MessageBox.Show(strErr);
                return -1;
            }
            else
            {

                return 0;
            }
        }

        private int camera_StopListen()
        {

            if (!CHCNetSDK.NET_DVR_StopListen_V30(iListenHandle))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                strErr = "停止监听失败，错误号：" + iLastErr; //撤防失败，输出错误号
                //MessageBox.Show(strErr);
                return -1;
            }
            else
            {
                //MessageBox.Show("停止监听！");
                return 0;
            }
        }
        //强制抓拍
        private int camera_ForceGetBigImage()
        {
            CHCNetSDK.NET_DVR_PLATE_RESULT struPlateResultInfo = new CHCNetSDK.NET_DVR_PLATE_RESULT();
            struPlateResultInfo.pBuffer1 = Marshal.AllocHGlobal(2 * 1024 * 1024);
            struPlateResultInfo.pBuffer2 = Marshal.AllocHGlobal(1024 * 1024);
            CHCNetSDK.NET_DVR_MANUALSNAP struInter = new CHCNetSDK.NET_DVR_MANUALSNAP();
            struInter.byLaneNo = 1;
            if (!CHCNetSDK.NET_DVR_ManualSnap(m_lUserID, ref struInter, ref struPlateResultInfo))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string strErr = "NET_DVR_ManualSnap failed, error code= " + iLastErr;
                AddOperLogCacheStr(strErr);

                Marshal.FreeHGlobal(struPlateResultInfo.pBuffer1);
                Marshal.FreeHGlobal(struPlateResultInfo.pBuffer2);
                return -1;
            }
            else
            {
                int iLen = (int)struPlateResultInfo.dwPicLen; ;
                if (iLen > 0)
                {
                    byte[] by = new byte[iLen];
                    if (struPlateResultInfo.struPlateInfo.sLicense.Equals("无车牌"))
                    {
                        GetPlateNo = "未检测";
                    }
                    else
                    {
                        string temp = "";
                        switch (struPlateResultInfo.struPlateInfo.byColor)
                        {
                            case 0:
                                temp = "蓝";
                                break;
                            case 1:
                                temp = "黄";
                                break;
                            case 2:
                                temp = "白";
                                break;
                            case 3:
                                temp = "黑";
                                break;
                            case 4:
                                temp = "绿";
                                break;
                            default:
                                break;
                        }
                        GetPlateNo = struPlateResultInfo.struPlateInfo.sLicense;
                    }
                    GetVehicleLogoRecog = "";
                    GetVehicleLogoRecog = CHCNetSDK.VLR_VEHICLE_CLASS[struPlateResultInfo.struVehicleInfo.byVehicleLogoRecog];

                    FlieClass fc = new FlieClass();
                    string dirpath = ".\\image\\";
                    DateTime forcetimedt = DateTime.Now;
                    string forcetime = forcetimedt.ToString("yyyyMMddHHmmss");
                    string imagename = forcetime + GetPlateNo + ".bmp";
                    dirpath += DateTime.Now.Year.ToString();
                    dirpath += "年\\";
                    dirpath += DateTime.Now.Month.ToString();
                    dirpath += "月\\";
                    dirpath += DateTime.Now.Day.ToString();
                    dirpath += "日\\";
                    imagepath = dirpath + imagename;
                    Marshal.Copy(struPlateResultInfo.pBuffer1, by, 0, iLen);
                    try
                    {
                        if (true == fc.WriteFileImage(dirpath, imagename, by, 0, iLen))
                        {
                            Marshal.FreeHGlobal(struPlateResultInfo.pBuffer1);
                            Marshal.FreeHGlobal(struPlateResultInfo.pBuffer2);
                            return 0;
                        }
                        else
                        {
                            Marshal.FreeHGlobal(struPlateResultInfo.pBuffer1);
                            Marshal.FreeHGlobal(struPlateResultInfo.pBuffer2);
                            //AddOperLogCacheStr("保存车牌图片失败!");
                            return -1;
                        }
                    }
                    catch (Exception ex)
                    {
                        //AddOperLogCacheStr("保存车牌图片失败!");
                        Marshal.FreeHGlobal(struPlateResultInfo.pBuffer1);
                        Marshal.FreeHGlobal(struPlateResultInfo.pBuffer2);
                        return -1;
                    }
                }
            }
            return 0;
        }
        public void MsgCallback(int lCommand, ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            //通过lCommand来判断接收到的报警信息类型，不同的lCommand对应不同的pAlarmInfo内容
            switch (lCommand)
            {
                case CHCNetSDK.COMM_ITS_PLATE_RESULT://交通抓拍结果上传(新报警信息类型)
                    ProcessCommAlarm_ITSPlate(ref pAlarmer, pAlarmInfo, dwBufLen, pUser);
                    break;
                default:
                    break;
            }
        }


        private uint ProcessCommAlarm_ITSPlate(ref CHCNetSDK.NET_DVR_ALARMER pAlarmer, IntPtr pAlarmInfo, uint dwBufLen, IntPtr pUser)
        {
            DateTime dtS = DateTime.Now;
            CHCNetSDK.NET_ITS_PLATE_RESULT struITSPlateResult = new CHCNetSDK.NET_ITS_PLATE_RESULT();
            uint dwSize = (uint)Marshal.SizeOf(struITSPlateResult);
            struITSPlateResult = (CHCNetSDK.NET_ITS_PLATE_RESULT)Marshal.PtrToStructure(pAlarmInfo, typeof(CHCNetSDK.NET_ITS_PLATE_RESULT));
            TimeSpan ts = DateTime.Now - dtS;
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 摄像机抓拍完成时间1：" + ts.TotalMilliseconds + "\r\n");
            AddOperLogCacheStr("进入报警布防回调函数,图片" + struITSPlateResult.dwPicNum.ToString() + "张..");
            string res = "成功";
            int iLen = (int)struITSPlateResult.struPicInfo[0].dwDataLen;
            byte[] by = new byte[iLen];
            if (iLen > 0) res = "成功";
            else res = "失败";
            AddOperLogCacheStr("取图返回:" + res);
            if (struITSPlateResult.struPlateInfo.sLicense.Equals("无车牌"))
            {
                GetPlateNo = "未检测";
            }
            else
            {
                string temp = "";
                switch (struITSPlateResult.struPlateInfo.byColor)
                {
                    case 0:
                        temp = "蓝";
                        break;
                    case 1:
                        temp = "黄";
                        break;
                    case 2:
                        temp = "白";
                        break;
                    case 3:
                        temp = "黑";
                        break;
                    case 4:
                        temp = "绿";
                        break;
                    default:
                        break;
                }
                GetPlateNo = struITSPlateResult.struPlateInfo.sLicense;
            }
            GetVehicleLogoRecog = "";
            GetVehicleLogoRecog = CHCNetSDK.VLR_VEHICLE_CLASS[struITSPlateResult.struVehicleInfo.byVehicleLogoRecog];
            FlieClass fc = new FlieClass();
            string dirpath = ".\\plateimage\\";
            DateTime forcetimedt = DateTime.Now;
            string forcetime = forcetimedt.ToString("yyyyMMddHHmmss");
            string imagename = forcetime + GetPlateNo + ".bmp";
            dirpath += DateTime.Now.Year.ToString();
            dirpath += "年\\";
            dirpath += DateTime.Now.Month.ToString();
            dirpath += "月\\";
            dirpath += DateTime.Now.Day.ToString();
            dirpath += "日\\";
            imagepath = dirpath + imagename;
            //暂时放这里
            ts = DateTime.Now - dtS;
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 摄像机抓拍完成时间2：" + ts.TotalMilliseconds + "\r\n");
            
            Marshal.Copy(struITSPlateResult.struPicInfo[0].pBuffer, by, 0, iLen);
            try
            {
                if (true == fc.WriteFileImage(dirpath, imagename, by, 0, iLen))
                {
                    CameraPicture.Set();
                    ts = DateTime.Now - dtS;
                    Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 摄像机抓拍完成时间3：" + ts.TotalMilliseconds + "\r\n");
                }
                else
                {
                    AddOperLogCacheStr("保存车牌图片失败!");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("保存车牌图片失败!");
                return 1;
            }
            return 0;
        }
        //布放回调函数（目前已取消）
        public uint CaremaCallBackFunction(int dwCom)
        {
            AddOperLogCacheStr("布防回调,图片" + dwCom.ToString() + "张..");
            string res = GetBigImage(1);
            GetPlateNo = HKcamera.GetPlateNo(1, 0);
            FlieClass fc = new FlieClass();
            string dirpath = string.Format("{0}\\{1}\\", Application.StartupPath, "PlatePic");
            DateTime forcetimedt = DateTime.Now;
            string forcetime = forcetimedt.ToString("yyyyMMddHHmmss");
            string imagename = forcetime + GetPlateNo + ".bmp";
            dirpath += DateTime.Now.Year.ToString();
            dirpath += "年\\";
            dirpath += DateTime.Now.Month.ToString();
            dirpath += "月\\";
            dirpath += DateTime.Now.Day.ToString();
            dirpath += "日\\";
            imagepath = dirpath + imagename;
            try
            {
                if (true == fc.WriteFileImage(dirpath, imagename, HKcamera.IdentInfo[1].VehImage, 0, HKcamera.IdentInfo[1].VehImageLen))
                {
                    //CameraPicture.Set();
                }
                else
                {
                    AddOperLogCacheStr("保存车牌图片失败!");
                    return 1;
                }
            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("保存车牌图片失败!");
                return 1;
            }
            return 0;
        }
        //取图，用于布放回调模式
        public string GetBigImage(int laneno)
        {
            int res;
            res = HKcamera.GetBigImage((byte)laneno, 0);
            if (res == 0)
            {
                return ("取图返回 成功");
            }
            else
            {
                return ("取图返回" + res.ToString());
            }
        }
        //强制抓拍
        public int ForceGetBigImage(int laneno)
        {
            int res = HKcamera.ForceGetLanePlate(laneno);
            if (res == 0)
            {

            }
            else
            {
                return res;
            }

            res = HKcamera.GetBigImage((byte)laneno, 0);

            if (res == 0)
            {

            }
            else
            {
                return res;
            }

            GetPlateNo = HKcamera.GetPlateNo(1, 0);
            FlieClass fc = new FlieClass();
            string dirpath = ".\\image\\";
            dirpath += DateTime.Now.Year.ToString();
            dirpath += "年\\";
            dirpath += DateTime.Now.Month.ToString();
            dirpath += "月\\";
            dirpath += DateTime.Now.Day.ToString();
            dirpath += "日\\";
            DateTime forcetimedt = DateTime.Now;
            string forcetime = forcetimedt.ToString("yyyyMMddHHmmss");
            string imagename = forcetime + GetPlateNo + ".bmp";
            imagepath = dirpath + imagename;
            forcetime = forcetimedt.ToString("yyyy-MM-dd HH:mm:ss");
            if (true == fc.WriteFileImage(dirpath, imagename, HKcamera.IdentInfo[laneno].VehImage, 0, HKcamera.IdentInfo[laneno].VehImageLen))
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        #endregion

         #region ******数据处理流程******
        //分包解包处理流程
        public void HanderOrgData(byte[] OrgBuff,int OrgBuffLen)
        {
            byte[] ReceiveBuf = new byte[OrgBuffLen];
            //数据处理，循环解析
            int gcnt = 0;
            //读取成功
            for (int i = 0; i < OrgBuffLen; i++)
            {
                //找到帧头
                if (gcnt == 0)
                {
                    if (OrgBuff[i] == 0xFF)
                    {
                        ReceiveBuf[0] = OrgBuff[i];
                        gcnt = 1;
                    }
                }
                else if (gcnt == 1)
                {
                    if (OrgBuff[i] == 0xFF)
                    {
                        ReceiveBuf[1] = OrgBuff[i];
                        gcnt = 2;
                    }
                    else
                    {
                        gcnt = 0;
                        continue;
                    }
                }
                else
                {
                    if (OrgBuff[i] == 0xFF)
                    {
                        ReceiveBuf[gcnt++] = OrgBuff[i];
                        if (gcnt > 3)
                        {
                            //数据处理流程
                            PreprocessRecvData(ReceiveBuf, gcnt);
                        }
                        gcnt = 0;
                    }
                    else
                    {
                        ReceiveBuf[gcnt++] = OrgBuff[i];
                        if (gcnt > 1024)//帧长超过1024，直接return
                        {
                            return;
                        }
                    }
                }
            }
        }
        //处理数据缓存的线程函数
        public void ResThread()
        {
            workThread = new Thread(ResHander);
            workThread.IsBackground = true;
            workThread.Priority = ThreadPriority.Highest;
            workThread.Start();
        }
        
        public void ResHander()
        {
            while (true)
            {
                StateObject Stateque = new StateObject();
                if (queue.TryDequeue(out Stateque))
                {
                    byte[] ReceiveBuf = new byte[Stateque.revLength];
                    //数据处理，循环解析
                    int gcnt = 0;
                    //读取成功
                    for (int i = 0; i < Stateque.revLength; i++)
                    {
                        //找到帧头
                        if (gcnt == 0)
                        {
                            if (Stateque.buffer[i] == 0xFF)
                            {
                                ReceiveBuf[0] = Stateque.buffer[i];
                                gcnt = 1;
                            }
                        }
                        else if (gcnt == 1)
                        {
                            if (Stateque.buffer[i] == 0xFF)
                            {
                                ReceiveBuf[1] = Stateque.buffer[i];
                                gcnt = 2;
                            }
                            else
                            {
                                gcnt = 0;
                                continue;
                            }
                        }
                        else
                        {
                            if (Stateque.buffer[i] == 0xFF)
                            {
                                ReceiveBuf[gcnt++] = Stateque.buffer[i];
                                if (gcnt > 3)
                                {
                                    //数据处理流程
                                    PreprocessRecvData(ReceiveBuf, gcnt);
                                }
                                gcnt = 0;
                            }
                            else
                            {
                                ReceiveBuf[gcnt++] = Stateque.buffer[i];
                                if (gcnt > 1024)//帧长超过1024，直接return
                                {
                                    return;
                                }
                            }
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        //处理数据缓存的线程函数
        public void QueueHanderThread()
        {
            queueThread = new Thread(QueueDataHanderFun3);
            queueThread.IsBackground = true;
            queueThread.Priority = ThreadPriority.Normal;
            queueThread.Start();
        }
       
        //第三版本数据匹配逻辑
        public void QueueDataHanderFun3()
        {
            bool isInRSUSql = false;
            bool isMarch = false;
            string sZuobistring = "";
            List<CarFullInfo> listAllCarInfo = new List<CarFullInfo>();
            Levenshtein LevenPercent = new Levenshtein();
            bool isGetbyCode = false;
            bool isGetbyPlate = false;
            string InsertString = "";
            string MarchFunction = "";

            while (true)//isUsingPiPei.DUIControls[1].Checked
            {
                try
                {
                    QueueRSUData qoutRSU = new QueueRSUData();
                    QueueJGData qoutJG = new QueueJGData();
                    isGetbyCode = false;
                    isGetbyPlate = false;
                    //先取ETC的数据
                    if (qRSUData.TryDequeue(out qoutRSU))
                    {
                        //先进行RSU数据存储
                        isInRSUSql = InsertRSUData(qoutRSU.qOBUPlateColor, qoutRSU.qOBUPlateNum, 
                            qoutRSU.qOBUMac, qoutRSU.qOBUY, qoutRSU.qOBUCarLength, qoutRSU.qOBUCarhigh, 
                            qoutRSU.qOBUCarType, qoutRSU.qOBUDateTime, qoutRSU.qRSURandCode.ToString("X2"));
                        if (!isInRSUSql)
                        {
                            //异常
                        }
                        for (int i = listAllCarInfo.Count - 1; i >= 0; i--)
                        {
                            //匹配规则
                            //1.车牌完全相同 2.位置相同且车牌是未识别 3.开启位置匹配且位置相同
                            //4.开启车牌模糊匹配且匹配度大于70%(由于车牌只有七位，三位不一致的时候相似度只有57%，)
                            //模糊匹配算法为Levenstein算法修改版，适用于字节丢失匹配
                            if (listAllCarInfo[i].sCamPlateNum == qoutRSU.qOBUPlateNum
                                || ((listAllCarInfo[i].sCamPlateNum == "未知" || listAllCarInfo[i].sCamPlateNum == "未检测")
                                && listAllCarInfo[i].sJGRandCode == qoutRSU.qRSURandCode.ToString("X2"))
                                || (OpenLocation.Checked && (listAllCarInfo[i].sJGRandCode == qoutRSU.qRSURandCode.ToString("X2")))
                                || (OpenMohu.Checked && (((int)(LevenPercent.LevenshteinDistancePercent(listAllCarInfo[i].sCamPlateNum, qoutRSU.qOBUPlateNum) * 100)) > 70)))
                            {
                                if(listAllCarInfo[i].sCamPlateNum == qoutRSU.qOBUPlateNum)
                                {
                                    MarchFunction="车牌匹配";
                                }
                                else if( ((listAllCarInfo[i].sCamPlateNum == "未知" || listAllCarInfo[i].sCamPlateNum == "未检测")
                                && listAllCarInfo[i].sJGRandCode == qoutRSU.qRSURandCode.ToString("X2")))
                                {
                                    MarchFunction="位置匹配1";
                                }
                                else if((OpenLocation.Checked && (listAllCarInfo[i].sJGRandCode == qoutRSU.qRSURandCode.ToString("X2"))))
                                {
                                    MarchFunction="位置匹配2";
                                }
                                else if ((OpenMohu.Checked && (((int)(LevenPercent.LevenshteinDistancePercent(listAllCarInfo[i].sCamPlateNum, qoutRSU.qOBUPlateNum) * 100)) > 70)))
                                {
                                    MarchFunction="模糊匹配";
                                }
                                listAllCarInfo[i].sOBUCarHigh = qoutRSU.qOBUCarhigh;
                                listAllCarInfo[i].sOBUCarLength = qoutRSU.qOBUCarLength;
                                listAllCarInfo[i].sOBUCartype = qoutRSU.qOBUCarType;
                                listAllCarInfo[i].sOBUDateTime = qoutRSU.qOBUDateTime;
                                listAllCarInfo[i].sOBUMac = qoutRSU.qOBUMac;
                                listAllCarInfo[i].sOBUPlateColor = qoutRSU.qOBUPlateColor;
                                listAllCarInfo[i].sOBUPlateNum = qoutRSU.qOBUPlateNum;
                                listAllCarInfo[i].sOBUY = qoutRSU.qOBUY;
                                listAllCarInfo[i].sRSURandCode = qoutRSU.qRSURandCode.ToString("X2");
                                listAllCarInfo[i].sCount = qoutRSU.qCount;
                                isMarch = true;
                                //界面显示
                                sZuobistring = MarchedShow(listAllCarInfo[i].sOBUCartype, listAllCarInfo[i].sOBUPlateNum, listAllCarInfo[i].sJGCarType, listAllCarInfo[i].sCamPlateNum, listAllCarInfo[i].sCamPicPath, listAllCarInfo[i].sCount);
                                //表格显示
                                adddataGridViewRoll(listAllCarInfo[i].sCount, listAllCarInfo[i].sJGCarType, listAllCarInfo[i].sOBUCartype,
                                    listAllCarInfo[i].sOBUDateTime, listAllCarInfo[i].sJGDateTime, listAllCarInfo[i].sOBUPlateNum,
                                    listAllCarInfo[i].sCamPlateNum, listAllCarInfo[i].sOBUPlateColor, listAllCarInfo[i].sCamPlateColor,
                                    listAllCarInfo[i].sCamBiao, listAllCarInfo[i].sJGId, listAllCarInfo[i].sOBUCarLength, listAllCarInfo[i].sOBUCarHigh, listAllCarInfo[i].sCamPicPath);
                                //更新数据库
                                //写入总数据库
                                InsertString = @"Insert into " + sql_dbname
                                    + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('"
                                    + listAllCarInfo[i].sJGCarLength + "','" + listAllCarInfo[i].sJGCarHigh + "','" + listAllCarInfo[i].sJGCarType + "','"
                                    + listAllCarInfo[i].sJGDateTime + "','" + listAllCarInfo[i].sCamPlateColor + "','" + listAllCarInfo[i].sCamPlateNum + "','"
                                    + listAllCarInfo[i].sCamBiao + "','" + listAllCarInfo[i].sCamPicPath + "','" + listAllCarInfo[i].sJGId + "','"
                                    + listAllCarInfo[i].sOBUPlateColor + "','" + listAllCarInfo[i].sOBUPlateNum + "','" + listAllCarInfo[i].sOBUMac + "','"
                                    + listAllCarInfo[i].sOBUY + "','" + listAllCarInfo[i].sOBUCarLength + "','" + listAllCarInfo[i].sOBUCarHigh + "','"
                                    + listAllCarInfo[i].sOBUCartype + "','" + listAllCarInfo[i].sOBUDateTime + "','" + sZuobistring + "','"
                                    + listAllCarInfo[i].sRSURandCode + "','" + "车牌匹配" + "')";
                                UpdateSQLData(InsertString);
                                listAllCarInfo.RemoveAt(i);
                                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + MarchFunction+"成功" + qoutRSU.qRSURandCode.ToString("X2") + "OBU车牌：" + qoutRSU.qOBUPlateNum + "\r\n");
                                break;
                            }
                        }
                       
                        if (!isMarch)
                        {
                            if (listAllCarInfo.Count >= 6)
                            {
                                //写入总数据库
                                InsertString = @"Insert into " + sql_dbname
                                    + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('"
                                    + listAllCarInfo[0].sJGCarLength + "','" + listAllCarInfo[0].sJGCarHigh + "','" + listAllCarInfo[0].sJGCarType + "','"
                                    + listAllCarInfo[0].sJGDateTime + "','" + listAllCarInfo[0].sCamPlateColor + "','" + listAllCarInfo[0].sCamPlateNum + "','"
                                    + listAllCarInfo[0].sCamBiao + "','" + listAllCarInfo[0].sCamPicPath + "','" + listAllCarInfo[0].sJGId + "','"
                                    + listAllCarInfo[0].sOBUPlateColor + "','" + listAllCarInfo[0].sOBUPlateNum + "','" + listAllCarInfo[0].sOBUMac + "','"
                                    + listAllCarInfo[0].sOBUY + "','" + listAllCarInfo[0].sOBUCarLength + "','" + listAllCarInfo[0].sOBUCarHigh + "','"
                                    + listAllCarInfo[0].sOBUCartype + "','" + listAllCarInfo[0].sOBUDateTime + "','" + "未知" + "','"
                                    + listAllCarInfo[0].sRSURandCode + "','" + "未能匹配" + "')";
                                UpdateSQLData(InsertString);
                                listAllCarInfo.RemoveAt(0);
                                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " RSU触发队列已满，首位清空" + "OBU车牌：" + listAllCarInfo[0].sOBUPlateNum + "识别车牌：" + listAllCarInfo[0].sCamPlateNum + "\r\n");
                            }
                            listAllCarInfo.Add(new CarFullInfo(qoutRSU.qOBUPlateNum,qoutRSU.qOBUCarType, qoutRSU.qRSURandCode.ToString("X2"), qoutRSU.qOBUDateTime,
                                qoutRSU.qOBUCarLength, qoutRSU.qOBUCarhigh, qoutRSU.qOBUY,qoutRSU.qOBUBiao, qoutRSU.qOBUPlateColor, qoutRSU.qOBUMac,
                                "", "", "", "", "", "", "", "", "", "", qoutRSU.qCount));
                            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " RSU入队列" + "车牌：" + qoutRSU.qOBUPlateNum + "\r\n");
                        }
                        else
                        {
                            isMarch = false;
                            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " RSU匹配JG车牌成功" + "车牌：" + qoutRSU.qOBUPlateNum + "入库Car表\r\n");

                        }
                    }
                    if (qJGData.TryDequeue(out qoutJG))
                    {
                        //入库
                        InsertJGData(qoutJG.qJGLength, qoutJG.qJGWide, qoutJG.qJGCarType, 
                            qoutJG.qJGId, qoutJG.qCamPlateNum, qoutJG.qCamPicPath, qoutJG.qJGDateTime, 
                            qoutJG.qCambiao, qoutJG.qCamPlateColor, qoutJG.qJGRandCode.ToString("X2"));
                        for (int i = listAllCarInfo.Count - 1; i >= 0; i--)
                        {
                            if (listAllCarInfo[i].sOBUPlateNum == qoutJG.qCamPlateNum
                                || ((listAllCarInfo[i].sRSURandCode == qoutJG.qJGRandCode.ToString("X2")) && qoutJG.qCamPlateNum == "未知")
                                || (OpenLocation.Checked && (listAllCarInfo[i].sRSURandCode == qoutJG.qJGRandCode.ToString("X2")))
                                || (OpenMohu.Checked && (((int)(LevenPercent.LevenshteinDistancePercent(listAllCarInfo[i].sOBUPlateNum, qoutJG.qCamPlateNum) * 100)) > 70)))
                            {
                                if (listAllCarInfo[i].sOBUPlateNum == qoutJG.qCamPlateNum)
                                {
                                    MarchFunction = "车牌匹配";
                                }
                                else if (((listAllCarInfo[i].sRSURandCode == qoutJG.qJGRandCode.ToString("X2")) && qoutJG.qCamPlateNum == "未知")
                                || (OpenLocation.Checked && (listAllCarInfo[i].sRSURandCode == qoutJG.qJGRandCode.ToString("X2"))))
                                {
                                    MarchFunction = "位置匹配1";
                                }
                                else if ((OpenLocation.Checked && (listAllCarInfo[i].sRSURandCode == qoutJG.qJGRandCode.ToString("X2"))))
                                {
                                    MarchFunction = "位置匹配2";
                                }
                                else if ((OpenMohu.Checked && (((int)(LevenPercent.LevenshteinDistancePercent(listAllCarInfo[i].sOBUPlateNum, qoutJG.qCamPlateNum) * 100)) > 70)))
                                {
                                    MarchFunction = "模糊匹配";
                                }
                                listAllCarInfo[i].sCamBiao = qoutJG.qCambiao;
                                listAllCarInfo[i].sCamPicPath = qoutJG.qCamPicPath;
                                listAllCarInfo[i].sCamPlateColor = qoutJG.qCamPlateColor;
                                listAllCarInfo[i].sCamPlateNum = qoutJG.qCamPlateNum;
                                listAllCarInfo[i].sJGCarHigh = qoutJG.qJGWide;
                                listAllCarInfo[i].sJGCarLength = qoutJG.qJGLength;
                                listAllCarInfo[i].sJGCarType = qoutJG.qJGCarType;
                                listAllCarInfo[i].sJGDateTime = qoutJG.qJGDateTime;
                                listAllCarInfo[i].sJGId = qoutJG.qJGId;
                                listAllCarInfo[i].sJGRandCode = qoutJG.qJGRandCode.ToString("X2");
                                isMarch = true;
                                //界面显示
                                sZuobistring = MarchedShow(listAllCarInfo[i].sOBUCartype, listAllCarInfo[i].sOBUPlateNum, listAllCarInfo[i].sJGCarType, listAllCarInfo[i].sCamPlateNum, listAllCarInfo[i].sCamPicPath, listAllCarInfo[i].sCount);
                                //表格显示
                                adddataGridViewRoll(listAllCarInfo[i].sCount, listAllCarInfo[i].sJGCarType, listAllCarInfo[i].sOBUCartype,
                                    listAllCarInfo[i].sOBUDateTime, listAllCarInfo[i].sJGDateTime, listAllCarInfo[i].sOBUPlateNum,
                                    listAllCarInfo[i].sCamPlateNum, listAllCarInfo[i].sOBUPlateColor, listAllCarInfo[i].sCamPlateColor,
                                    listAllCarInfo[i].sCamBiao, listAllCarInfo[i].sJGId, listAllCarInfo[i].sOBUCarLength, listAllCarInfo[i].sOBUCarHigh, listAllCarInfo[i].sCamPicPath);
                                //写入总数据库
                                InsertString = @"Insert into " + sql_dbname
                                    + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('"
                                    + listAllCarInfo[i].sJGCarLength + "','" + listAllCarInfo[i].sJGCarHigh + "','" + listAllCarInfo[i].sJGCarType + "','"
                                    + listAllCarInfo[i].sJGDateTime + "','" + listAllCarInfo[i].sCamPlateColor + "','" + listAllCarInfo[i].sCamPlateNum + "','"
                                    + listAllCarInfo[i].sCamBiao + "','" + listAllCarInfo[i].sCamPicPath + "','" + listAllCarInfo[i].sJGId + "','"
                                    + listAllCarInfo[i].sOBUPlateColor + "','" + listAllCarInfo[i].sOBUPlateNum + "','" + listAllCarInfo[i].sOBUMac + "','"
                                    + listAllCarInfo[i].sOBUY + "','" + listAllCarInfo[i].sOBUCarLength + "','" + listAllCarInfo[i].sOBUCarHigh + "','"
                                    + listAllCarInfo[i].sOBUCartype + "','" + listAllCarInfo[i].sOBUDateTime + "','" + sZuobistring + "','"
                                    + listAllCarInfo[i].sRSURandCode + "','" + MarchFunction + "')";
                                UpdateSQLData(InsertString);
                                listAllCarInfo.RemoveAt(i);
                                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + MarchFunction+"成功" + "识别车牌：" + qoutJG.qCamPlateNum + "\r\n");
                                //弱强制匹配
                                if (i >= 2)
                                {
                                    if (listAllCarInfo[i - 2].sOBUPlateNum != "" && listAllCarInfo[i - 1].sCamPlateNum != "")
                                    {
                                        MarchFunction = "强制匹配";
                                        //界面显示（暂时不更新吧？）
                                        //sZuobistring = MarchedShow(listAllCarInfo[i-2].sOBUCartype, listAllCarInfo[i-2].sOBUPlateNum, listAllCarInfo[i-1].sJGCarType, listAllCarInfo[i-1].sCamPlateNum, listAllCarInfo[i-1].sCamPicPath, listAllCarInfo[i-2].sCount);
                                        //表格显示
                                        adddataGridViewRoll(listAllCarInfo[i-2].sCount, listAllCarInfo[i-1].sJGCarType, listAllCarInfo[i-2].sOBUCartype,
                                            listAllCarInfo[i-2].sOBUDateTime, listAllCarInfo[i-1].sJGDateTime, listAllCarInfo[i-2].sOBUPlateNum,
                                            listAllCarInfo[i-1].sCamPlateNum, listAllCarInfo[i-2].sOBUPlateColor, listAllCarInfo[i-1].sCamPlateColor,
                                            listAllCarInfo[i-1].sCamBiao, listAllCarInfo[i-1].sJGId, listAllCarInfo[i-2].sOBUCarLength, listAllCarInfo[i-2].sOBUCarHigh, listAllCarInfo[i-1].sCamPicPath);
                                        //更新数据库
                                        //写入总数据库
                                        InsertString = @"Insert into " + sql_dbname
                                            + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('"
                                            + listAllCarInfo[i-1].sJGCarLength + "','" + listAllCarInfo[i-1].sJGCarHigh + "','" + listAllCarInfo[i-1].sJGCarType + "','"
                                            + listAllCarInfo[i-1].sJGDateTime + "','" + listAllCarInfo[i-1].sCamPlateColor + "','" + listAllCarInfo[i-1].sCamPlateNum + "','"
                                            + listAllCarInfo[i-1].sCamBiao + "','" + listAllCarInfo[i-1].sCamPicPath + "','" + listAllCarInfo[i-1].sJGId + "','"
                                            + listAllCarInfo[i-2].sOBUPlateColor + "','" + listAllCarInfo[i-2].sOBUPlateNum + "','" + listAllCarInfo[i-2].sOBUMac + "','"
                                            + listAllCarInfo[i-2].sOBUY + "','" + listAllCarInfo[i-2].sOBUCarLength + "','" + listAllCarInfo[i-2].sOBUCarHigh + "','"
                                            + listAllCarInfo[i-2].sOBUCartype + "','" + listAllCarInfo[i-2].sOBUDateTime + "','" + "强制匹配作弊不详" + "','"
                                            + listAllCarInfo[i-2].sRSURandCode + "','" + "强制匹配" + "')";
                                        UpdateSQLData(InsertString);
                                        
                                        Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + MarchFunction + "成功" + listAllCarInfo[i - 2].sRSURandCode + "OBU车牌：" + listAllCarInfo[i - 2].sOBUPlateNum + "\r\n");
                                        listAllCarInfo.RemoveAt(i - 1);
                                        listAllCarInfo.RemoveAt(i - 2);
                                    }
                                }

                                break;
                            }
                        }
                        if (!isMarch)
                        {
                            if (listAllCarInfo.Count >= 6)
                            {
                                //写入总数据库
                                InsertString = @"Insert into " + sql_dbname
                                    + ".dbo.CarInfo(JGLength,JGWide,JGCarType,ForceTime,CamPlateColor,CamPlateNum,Cambiao,CamPicPath,JGId,OBUPlateColor,OBUPlateNum,OBUMac,OBUY,OBUCarLength,OBUCarHigh,OBUCarType,TradeTime,TradeState,RandCode,GetFunction) values('"
                                    + listAllCarInfo[0].sJGCarLength + "','" + listAllCarInfo[0].sJGCarHigh + "','" + listAllCarInfo[0].sJGCarType + "','"
                                    + listAllCarInfo[0].sJGDateTime + "','" + listAllCarInfo[0].sCamPlateColor + "','" + listAllCarInfo[0].sCamPlateNum + "','"
                                    + listAllCarInfo[0].sCamBiao + "','" + listAllCarInfo[0].sCamPicPath + "','" + listAllCarInfo[0].sJGId + "','"
                                    + listAllCarInfo[0].sOBUPlateColor + "','" + listAllCarInfo[0].sOBUPlateNum + "','" + listAllCarInfo[0].sOBUMac + "','"
                                    + listAllCarInfo[0].sOBUY + "','" + listAllCarInfo[0].sOBUCarLength + "','" + listAllCarInfo[0].sOBUCarHigh + "','"
                                    + listAllCarInfo[0].sOBUCartype + "','" + listAllCarInfo[0].sOBUDateTime + "','" + "未知" + "','"
                                    + listAllCarInfo[0].sRSURandCode + "','" + "未能匹配" + "')";
                                UpdateSQLData(InsertString);
                                listAllCarInfo.RemoveAt(0);
                                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " RSU触发队列已满，首位清空" + "OBU车牌：" + listAllCarInfo[0].sOBUPlateNum + "识别车牌：" + listAllCarInfo[0].sCamPlateNum + "\r\n");
                            }
                            listAllCarInfo.Add(new CarFullInfo("", "", "", "", "", "", "", "","", "", qoutJG.qJGCarType,
                                qoutJG.qJGWide, qoutJG.qJGLength, qoutJG.qJGDateTime, qoutJG.qJGId, qoutJG.qCamPlateNum,
                                qoutJG.qCamPlateColor, qoutJG.qCambiao, qoutJG.qCamPicPath, qoutJG.qJGRandCode.ToString("X2"),""));
                            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " JG入队列" + "车牌：" + qoutJG.qCamPlateNum + "\r\n");
                        }
                        else
                        {
                            isMarch = false;
                            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " JG匹配RSU车牌成功，跟随码" + qoutJG.qJGRandCode.ToString("X2") + "车牌：" + qoutJG.qCamPlateNum + "入库Car表\r\n");
                        }
                    }
                    

                }
                catch (Exception ex)
                {
                    Log.WriteLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 数据匹配异常\r\n" + ex.ToString() + "\r\n");
                }
                Thread.Sleep(100);
            }
        }
        //已匹配数据显示
        public string MarchedShow(string m_sOBUCarType, string m_sOBUPlateNum, string m_sJGCarType, string m_sCamPlateNum, string m_sPicPath, string m_sCarCount)
        {
            string sZuobiString = "";
            int iJGCarType = 0;
            int iOBUCarType = 0; 
            if (m_sJGCarType == "未知"||m_sJGCarType=="未检测"||m_sJGCarType==""||m_sJGCarType==null)
            {
                sZuobiString = "作弊未知";
                iJGCarType = 0;
                iOBUCarType = Convert.ToInt16(m_sOBUCarType.Substring(1));
            }
            else
            {
                iJGCarType = Convert.ToInt16(m_sJGCarType.Substring(1));
                iOBUCarType = Convert.ToInt16(m_sOBUCarType.Substring(1));
                if (iJGCarType <= iOBUCarType)
                {
                    m_sJGCarType = m_sOBUCarType;
                    sZuobiString = "正常通车";
                }
                else
                {
                    sZuobiString = "可能作弊";
                }
            }
            if (m_sPicPath != "未知"&&m_sPicPath!=""&&m_sPicPath!=null)
            {
                pictureBoxVehshow(m_sPicPath);//显示图片
            }
            plateNoshow(m_sOBUPlateNum, m_sOBUCarType, m_sCamPlateNum, m_sJGCarType, m_sCarCount);
            return sZuobiString;
        }
        //数据处理函数
        private void PreprocessRecvData(byte[] p_pBuffer, int p_nLen)//
        {
            int ret = datah.DataEncoding(ref p_pBuffer, ref p_nLen);
            if (ret != 0)
            {
                return;
            }
            switch (p_pBuffer[3])
            {
                case 0x81:
                    //激光数据
                    TcpReply(0x18, jg_sock);
                    HanderJGData(p_pBuffer, p_nLen);
                    break;
                //case 0x9D:
                //    //RSU的心跳
                //    TcpReply(0xD9, rsu_sock);
                //    break;
                case 0xD8:
                    //激光位置
                    SaveLocation(p_pBuffer, p_nLen);
                    break;
                case 0x7D:
                    //ETC数据
                    TcpReply(0xD7, rsu_sock);
                    HanderRSUData(p_pBuffer, p_nLen);
                    break;
                case 0x82:
                    //通知摄像机即将抓拍
                    HanderJGStartCam(p_pBuffer, p_nLen);
                    break;
                default:
                    break;
            }
        }

        public void SaveLocation(byte[] buffer, int bufferlen)
        {
            string ss = "";
            string location = "";
            for (int i = 0; i < bufferlen; i++)
            {
                ss += buffer[i].ToString("X2");
                ss += " ";
            }
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "收到激光的位置数据" + ss + "\r\n");
            AddOperLogCacheStr("收到激光返回的位置数据" + ss);
            for (int i = 0; i < 8; i++)
            {
                location += ((long)(buffer[(i + 1) * 4] << 24 | buffer[(i + 1) * 4 + 1] << 16 | buffer[(i + 1) * 4 + 2] << 8 | buffer[(i + 1) * 4 + 3])).ToString();
                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "第" + i + "个位置" + location + "\r\n");
                location = "";
            }
        }

        public void TcpReply(byte command, Socket sk)
        {

            int send_lenth = 0;
            byte[] send_buffer;
            send_buffer = new byte[100];
            send_buffer[send_lenth++] = command;
            send_buffer[send_lenth++] = 0x00;
            datah.DataCoding(ref send_buffer, ref send_lenth);
            if (sk == jg_sock)
            {
                if (IsConnJG == false)
                    return;
                try
                {
                    jg_sock.Send(send_buffer, 0, send_lenth, 0);
                }
                catch (Exception ex)
                {
                    AddOperLogCacheStr("回复激光异常" + ex.ToString());
                }
            }
            else if (sk == rsu_sock)
            {
                if (IsConnRSU == false)
                    return;
                try
                {
                    rsu_sock.Send(send_buffer, 0, send_lenth, 0);
                }
                catch (Exception ex)
                {
                    AddOperLogCacheStr("回复RSU异常" + ex.ToString());
                }
            }

        }

        public void SendLocation(ushort obuy, long randCode)
        {
            int send_lenth = 0;
            byte[] send_buffer;
            send_buffer = new byte[100];
            send_buffer[send_lenth++] = 0x8D;
            send_buffer[send_lenth++] = (byte)(obuy >> 8);
            send_buffer[send_lenth++] = (byte)(obuy);
            send_buffer[send_lenth++] = (byte)(randCode >> 24);
            send_buffer[send_lenth++] = (byte)(randCode >> 16);
            send_buffer[send_lenth++] = (byte)(randCode >> 8);
            send_buffer[send_lenth++] = (byte)(randCode);
            datah.DataCoding(ref send_buffer, ref send_lenth);
            if (IsConnJG == false)
                return;
            try
            {
                string sss = "";
                jg_sock.Send(send_buffer, 0, send_lenth, 0);
                for (int i = 0; i < send_lenth; i++)
                {
                    sss += send_buffer[i].ToString("X2");
                    sss += " ";
                }
                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 发送位置:" + sss + "\r\n");

            }
            catch (Exception ex)
            {
                AddOperLogCacheStr("发送位置异常" + ex.ToString());
            }
        }
        #endregion

        #region******激光数据解析与加入队列******
        public void HanderJGData(byte[] databuff, int bufflen)
        {
           
            int st = 2;
            string ss = "";
            for (int i = 0; i < bufflen; i++)
            {
                ss += databuff[i].ToString("X2");
                ss += " ";
            }
            AddOperLogCacheStr("收到激光数据" + ss);
            QueueJGData m_qJG = new QueueJGData();
            bool match_flag = false;//匹配标识
            m_qJG.qJGId = ((ushort)(databuff[3 + st] << 8 | databuff[4 + st])).ToString();
            foreach (var camlist in listCamInfo)
            {
                if (camlist.qJGID == m_qJG.qJGId)
                {
                    m_qJG.qCamPlateNum = camlist.qCamPlateNum;
                    m_qJG.qCamPlateColor = camlist.qCamPlateColor;
                    m_qJG.qCambiao = camlist.qCambiao;
                    m_qJG.qCamPicPath = camlist.qCamPicPath;
                    match_flag = true;
                    break;
                }
            }
            m_qJG.qJGCarType = databuff[5 + st].ToString();
            m_qJG.qJGLength = ((ushort)(databuff[9 + st] << 8 | databuff[10 + st])).ToString();
            m_qJG.qJGWide = ((ushort)(databuff[11 + st] << 8 | databuff[12 + st])).ToString();
            m_qJG.qJGDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            m_qJG.qJGRandCode = (long)(databuff[13 + st] << 24 | databuff[14 + st] << 16 | databuff[15 + st] << 8 | databuff[16 + st]);
            //1~4 客1~4；5~11 货1~5
            switch (databuff[5 + st])
            {
                case 1:
                    m_qJG.qJGCarType = "客1";
                    break;
                case 2:
                    m_qJG.qJGCarType = "客2";
                    break;
                case 3:
                    m_qJG.qJGCarType = "客3";
                    break;
                case 4:
                    m_qJG.qJGCarType = "客4";
                    break;
                case 5:
                    m_qJG.qJGCarType = "货1";
                    break;
                case 6:
                    m_qJG.qJGCarType = "货2";
                    break;
                case 7:
                    m_qJG.qJGCarType = "货3";
                    break;
                case 8:
                    m_qJG.qJGCarType = "货4";
                    break;
                case 9:
                    m_qJG.qJGCarType = "货5";
                    break;
                case 10:
                    m_qJG.qJGCarType = "货6";
                    break;
                case 11:
                    m_qJG.qJGCarType = "货7";
                    break;
                default:
                    m_qJG.qJGCarType = "未知";
                    break;
            }
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  激光入栈" 
                +"车牌："+ m_qJG.qCamPlateNum +"车型"+m_qJG.qJGCarType+"车长："+m_qJG.qJGLength+"车高："
                +m_qJG.qJGWide+"车标："+m_qJG.qCambiao+"激光ID"+m_qJG.qJGId+"随机码："+m_qJG.qJGRandCode.ToString("X2")+ "\r\n");
            //Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 激光入栈原始数据：" + ss + "\r\n");
            lock (qJGData)
            {
                qJGData.Enqueue(m_qJG);//放入激光的缓存中
            }
            jg_inQueueDone.Set();
        }
        #endregion

        #region******通知摄像机抓拍******
        public void HanderJGStartCam(byte[] databuff, int bufflen)
        {
            int st = 2;
            QueueJGData m_qJG = new QueueJGData();
            bool match_flag = false;//匹配标识
            m_qJG.qJGId = ((ushort)(databuff[3 + st] << 8 | databuff[4 + st])).ToString();
            if (OpenForce.Checked)
            {
                int res = camera_ForceGetBigImage();
                if (res == 0)
                {
                    match_flag = true;
                    m_qJG.qCamPicPath = imagepath;
                    AddOperLogCacheStr("强制抓拍成功");
                }
                else
                {
                    match_flag = false;
                    m_qJG.qCamPicPath = "未知";
                    AddOperLogCacheStr("强制抓拍失败");
                }
            }
            else
            {
                CameraPicture.Reset();
                if (CameraPicture.WaitOne(1400))
                {
                    match_flag = true;//匹配成功 
                    m_qJG.qCamPicPath = imagepath;
                }
                else
                {
                    match_flag = false;//匹配失败
                    m_qJG.qCamPicPath = "未知";
                }
            }
            
            
            if (match_flag == true)
            {
                if (GetPlateNo == "未检测")
                {
                    m_qJG.qCamPlateColor = "未检测";
                    m_qJG.qCamPlateNum = GetPlateNo;
                    m_qJG.qCambiao = "未知";
                }
                else
                {
                    if (GetPlateNo.Length > 3)
                    {
                        m_qJG.qCamPlateColor = GetPlateNo.Substring(0, 1);
                        m_qJG.qCamPlateNum = GetPlateNo.Substring(1);
                        m_qJG.qCambiao = GetVehicleLogoRecog;
                    }
                }
                m_qJG.qCamPicPath = imagepath;
            }
            else
            {
                m_qJG.qCamPlateColor = "未知";
                m_qJG.qCamPlateNum = "未知";
                m_qJG.qCamPicPath = "未知";
                m_qJG.qCambiao = "未知";
                m_qJG.qCamPicPath = "未知";
                m_qJG.qCamPicPath = "未知";
            }
            if (string.Equals(m_qJG.qCamPlateColor.ToString(), "黄"))
            {
                if (databuff[5 + st] == 1)
                    databuff[5 + st] = 2;
            }
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  摄像机抓拍"
                + "车牌：" + m_qJG.qCamPlateNum + "车标：" + m_qJG.qCambiao + "激光ID" + m_qJG.qJGId + "\r\n");
            lock (listCamInfo)
            {
                if (listCamInfo.Count >= 6)
                {
                    listCamInfo.RemoveRange(0, 1);
                }
                listCamInfo.Add(new CamList(m_qJG.qJGId, m_qJG.qCamPlateNum, m_qJG.qCamPlateColor,m_qJG.qCamPicPath,m_qJG.qCambiao));
            }
        }
        #endregion

        #region******天线数据解析与加入队列******
        public void HanderRSUData(byte[] databuff, int bufflen)
        {
            int st = 2;
            string ss = "";
            for (int i = 0; i < bufflen; i++)
            {
                ss += databuff[i].ToString("X2");
                ss += " ";
            }
            AddOperLogCacheStr("收到天线数据" + ss);
            QueueRSUData m_qRSU = new QueueRSUData();
            m_qRSU.qOBUMac = databuff[2 + st].ToString("X2") + databuff[3 + st].ToString("X2") + databuff[4 + st].ToString("X2") + databuff[5 + st].ToString("X2");
            m_qRSU.qOBUY = ((ushort)(databuff[8 + st] << 8 | databuff[9 + st])).ToString();
            int sit = 0;
            m_qRSU.qOBUPlateNum = databuff[10 + st].ToString("X2") + databuff[11 + st].ToString("X2") + databuff[12 + st].ToString("X2") + databuff[13 + st].ToString("X2") + databuff[14 + st].ToString("X2") + databuff[15 + st].ToString("X2") + databuff[16 + st].ToString("X2");
            while (databuff[16 + sit + 1+st] != 0x00)
            {
                m_qRSU.qOBUPlateNum += databuff[17 + sit+st].ToString("X2");
                sit++;
            }
            //m_qRSU.qOBUPlateNum = databuff[10 + st].ToString("X2") + databuff[11 + st].ToString("X2") + databuff[12 + st].ToString("X2") + databuff[13 + st].ToString("X2") + databuff[14 + st].ToString("X2") + databuff[15 + st].ToString("X2") + databuff[16 + st].ToString("X2") + databuff[17 + st].ToString("X2") + databuff[18 + st].ToString("X2") + databuff[19 + st].ToString("X2") + databuff[20 + st].ToString("X2") + databuff[21 + st].ToString("X2") + databuff[22 + st].ToString("X2");
            m_qRSU.qOBUPlateNum = Encoding.GetEncoding("GB2312").GetString(HexStringToByteArray(m_qRSU.qOBUPlateNum));
            switch (databuff[23 + st])
            {
                case 0:
                    m_qRSU.qOBUPlateColor = "蓝色";
                    break;
                case 1:
                    m_qRSU.qOBUPlateColor = "黄色";
                    break;
                case 2:
                    m_qRSU.qOBUPlateColor = "黑色";
                    break;
                case 3:
                    m_qRSU.qOBUPlateColor = "白色";
                    break;
                default:
                    m_qRSU.qOBUPlateColor = "未知";
                    break;
            }
            m_qRSU.qOBUCarLength = ((ushort)(databuff[25 + st] << 8 | databuff[26 + st])).ToString()+"00";
            m_qRSU.qOBUCarhigh = databuff[28 + st].ToString()+"00";
            m_qRSU.qOBUBiao="";
            for (int i = 0; i < 16; i++)
            {
                m_qRSU.qOBUBiao += databuff[40 + i].ToString("X2");
            
            }
            m_qRSU.qOBUBiao = Encoding.GetEncoding("GB2312").GetString(HexStringToByteArray(m_qRSU.qOBUBiao));
            m_qRSU.qOBUDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff");
            g_lUnixTime = GetUnixTime();
            m_qRSU.qRSURandCode = g_lUnixTime;
            //SendLocation((ushort)(databuff[8 + st] << 8 | databuff[9 + st]), m_qRSU.qRSURandCode);
            SendLocation(0xfdfd, m_qRSU.qRSURandCode);
            switch (databuff[24 + st])
            {
                case 1:
                    m_qRSU.qOBUCarType = "客1";
                    break;
                case 2:
                    m_qRSU.qOBUCarType = "客2";
                    break;
                case 3:
                    m_qRSU.qOBUCarType = "客3";
                    break;
                case 4:
                    m_qRSU.qOBUCarType = "客4";
                    break;
                case 5:
                    m_qRSU.qOBUCarType = "货1";
                    break;
                case 6:
                    m_qRSU.qOBUCarType = "货2";
                    break;
                case 7:
                    m_qRSU.qOBUCarType = "货3";
                    break;
                case 8:
                    m_qRSU.qOBUCarType = "货4";
                    break;
                case 9:
                    m_qRSU.qOBUCarType = "货5";
                    break;
                case 10:
                    m_qRSU.qOBUCarType = "货6";
                    break;
                case 11:
                    m_qRSU.qOBUCarType = "货7";
                    break;
                default:
                    m_qRSU.qOBUCarType = "未知";
                    break;
            }
            Count++;
            m_qRSU.qCount = Count.ToString();
            Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + "  天线入栈:" 
                + " 车牌："+m_qRSU.qOBUPlateNum+" 车型："+m_qRSU.qOBUCarType+" OBUY位置："
                +m_qRSU.qOBUY+" 随机码："+m_qRSU.qRSURandCode.ToString("X2") + "\r\n");
            //Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff") + " 天线入栈原始数据：" + ss + "\r\n");
            lock (qRSUData)
            {
                qRSUData.Enqueue(m_qRSU);//列入RSU数据缓存中
            }

            rsu_inQueueDone.Set();
        }
        public static byte[] HexStringToByteArray(string s)
        {
            s = s.Replace(" ", "").Trim().ToUpper();
            byte[] buffer = new byte[s.Length / 2];
            for (int i = 0; i < s.Length; i += 2)
                buffer[i / 2] = (byte)Convert.ToByte(s.Substring(i, 2), 16);
            return buffer;
        }

        public long GetUnixTime()
        {
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1, 0, 0, 0));
            DateTime nowTime = DateTime.Now;
            long unixTime = (long)Math.Round((nowTime - startTime).TotalSeconds, MidpointRounding.AwayFromZero);
            return unixTime;
        }
        #endregion


        private void qqButton3_Click(object sender, EventArgs e)
        {
            QQMessageBox.Show(
                this,
                "更改信息成功a！",
                "提示",
                QQMessageBoxIcon.OK,
                QQMessageBoxButtons.OK);
        }

        #region******数据结果导出******
        private void button1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();
            saveFileDialog1.FileName = DateTime.Now.ToString("yyyyMMdd HHmm");
            string PathExcel = "";
            saveFileDialog1.Filter = "Excel files(*.xls)|*.xls";
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {

                PathExcel = saveFileDialog1.FileName;
                ExcelCreate excre = new ExcelCreate();
                DataTable dt = new DataTable();
                dt.Columns.Add("序号");
                dt.Columns.Add("交易时间");
                dt.Columns.Add("车牌号码");
                dt.Columns.Add("OBU车型");
                dt.Columns.Add("检测车型");
                dt.Columns.Add("可能作弊");
                dt.Columns.Add("车长");
                dt.Columns.Add("车高");
                dt.Columns.Add("图片路径");

                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    DataRow dr = dt.NewRow();
                    dr["序号"] = (dataGridView1.Rows[i].Cells[0].Value);
                    dr["交易时间"] = (dataGridView1.Rows[i].Cells[1].Value);
                    dr["车牌号码"] = (dataGridView1.Rows[i].Cells[2].Value);
                    dr["OBU车型"] = (dataGridView1.Rows[i].Cells[3].Value);
                    dr["检测车型"] = (dataGridView1.Rows[i].Cells[4].Value);
                    dr["可能作弊"] = (dataGridView1.Rows[i].Cells[5].Value);
                    dr["车长"] = (dataGridView1.Rows[i].Cells[6].Value);
                    dr["车高"] = (dataGridView1.Rows[i].Cells[7].Value);
                    dr["图片路径"] = (dataGridView1.Rows[i].Cells[8].Value);

                    dt.Rows.Add(dr);
                }

                string dt12 = DateTime.Now.ToString("yyyyMMddhhmmss");

                excre.OutFileToDisk(dt, "车辆信息数据表", PathExcel);
                MessageBox.Show("Execl导出成功，路劲为：" + PathExcel);
            }
        }

        private void querybutton_Click(object sender, EventArgs e)
        {
            string s_Id;
            string s_RsuTradeTime;
            string s_RsuPlateNum;
            string s_RsuCarType;
            string s_JgCarType;
            string s_IsZuobi;
            string s_JgLength;
            string s_JgWide;
            string s_CamPicPath;
            this.dataGridView1.Rows.Clear();
            try
            {
                if (this.dateEndTime.Value < this.dateStartTime.Value)
                {
                    MessageBox.Show("结束时间必须大于起始时间");
                    return;
                }
                string limit_select = " ";
                if (checkBox1.CheckState == CheckState.Checked)
                {
                    limit_select = " and TradeState='可能作弊'";
                }
                string CarSerch = "select * from " + sql_dbname + ".dbo.CarInfo where TradeTime > '" + this.dateStartTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "' and TradeTime < '" + this.dateEndTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'" + limit_select + "  order by ID";//车道号

                SqlDataReader sdr = ExecuteQuery(CarSerch);
                bool flag1 = false;
                while (sdr.Read())
                {
                    s_Id = sdr[0].ToString();
                    s_RsuTradeTime = sdr[15].ToString();
                    s_RsuPlateNum = sdr[11].ToString();
                    s_RsuCarType = sdr[14].ToString();
                    s_JgCarType = sdr[3].ToString();
                    s_IsZuobi = sdr[16].ToString();
                    s_JgLength = sdr[1].ToString();
                    s_JgWide = sdr[2].ToString();
                    s_CamPicPath = sdr[8].ToString();
                    DelegateState.InsertGridview(s_Id, s_RsuTradeTime, s_RsuPlateNum, s_RsuCarType, s_JgCarType, s_IsZuobi, s_JgLength, s_JgWide, s_CamPicPath);
                    flag1 = true;
                }
                if (flag1 == false)
                    MessageBox.Show("查询完成，没有数据");
            }
            catch (SystemException ex)
            {
                Log.WriteLog(DateTime.Now + "\r\n" + "数据库查询失败\r\n" + ex.ToString() + "\r\n");
                MessageBox.Show("查询失败" + ex.ToString());
            }
        }
        #endregion

        private void qqButton1_Click(object sender, EventArgs e)
        {
           // int a = isUsingPiPei.DUIControls.Count;
        }
        //心跳定时检测
        private void timer1_Tick(object sender, EventArgs e)
        {
            HeartJGCount++;
            HeartRSUCount++;
            //if (IsConnJG)
            //{
            TcpReply(0x9D, jg_sock);
            //}
            if (HeartJGCount >= 5)
            {
                IsConnJG = false;
                //链接断开了
                jg_sock.Close();
                IsConnJG = false;
                HeartJGCount = 0;
                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 激光控制器心跳检测超时，激光通信断开重连\r\n");
            }
            if (HeartRSUCount >= 5)
            {
                IsConnRSU = false;
                rsu_sock.Close();
                IsConnRSU = false;
                HeartRSUCount = 0;
                Log.WritePlateLog(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " 天线控制器心跳检测超时，天线通信断开重连\r\n");
            }
        }
        //窗口关闭退出环境，防止残留线程
        private void FormDemo_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
            Environment.Exit(0);
        }


    }
}
