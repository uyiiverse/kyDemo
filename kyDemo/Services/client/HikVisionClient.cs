using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Text;
using System.IO;

using System.Runtime.InteropServices;
using kyDemo;

public class HikVisionConnectionManager
{
    private static HikVisionConnectionManager _instance;
    public string serverIp { get; set; }
    public string serverPort { get; set; }
    public string serverUserName { get; set; }
    public string serverPassword { get; set; }

    private uint iLastErr = 0;
    private Int32 m_lUserID = -1;
    private bool m_bInitSDK = false;
    private bool m_bRecord = false;
    private bool m_bTalk = false;
    private Int32 m_lRealHandle = -1;
    private int lVoiceComHandle = -1;
    private string str;

    CHCNetSDK.REALDATACALLBACK RealData = null;
    CHCNetSDK.LOGINRESULTCALLBACK LoginCallBack = null;
    public CHCNetSDK.NET_DVR_PTZPOS m_struPtzCfg;
    public CHCNetSDK.NET_DVR_USER_LOGIN_INFO struLogInfo;
    public CHCNetSDK.NET_DVR_DEVICEINFO_V40 DeviceInfo;

    private HikVisionConnectionManager()
    {
        m_bInitSDK = CHCNetSDK.NET_DVR_Init();
        if (m_bInitSDK == false)
        {
            MessageBox.Show("NET_DVR_Init error!");
            return;
        }
        else
        {
        }
    }

    public static HikVisionConnectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new HikVisionConnectionManager();
            }
            return _instance;
        }
    }

    public void SetIPAndPort(string ip, string port, string username, string password)
    {
        serverIp = ip;
        serverPort = port;
        serverUserName = username;
        serverPassword = password;
    }


    public void cbLoginCallBack(int lUserID, int dwResult, IntPtr lpDeviceInfo, IntPtr pUser)
    {
        string strLoginCallBack = "登录设备，lUserID：" + lUserID + "，dwResult：" + dwResult;

        if (dwResult == 0)
        {
            uint iErrCode = CHCNetSDK.NET_DVR_GetLastError();
            strLoginCallBack = strLoginCallBack + "，错误号:" + iErrCode;
        }

    }

    public void Connect()
    {
        if (m_lUserID < 0)
        {

            struLogInfo = new CHCNetSDK.NET_DVR_USER_LOGIN_INFO();

            //设备IP地址或者域名
            byte[] byIP = System.Text.Encoding.Default.GetBytes(serverIp);
            struLogInfo.sDeviceAddress = new byte[129];
            byIP.CopyTo(struLogInfo.sDeviceAddress, 0);

            //设备用户名
            byte[] byUserName = System.Text.Encoding.Default.GetBytes(serverUserName);
            struLogInfo.sUserName = new byte[64];
            byUserName.CopyTo(struLogInfo.sUserName, 0);

            //设备密码
            byte[] byPassword = System.Text.Encoding.Default.GetBytes(serverPassword);
            struLogInfo.sPassword = new byte[64];
            byPassword.CopyTo(struLogInfo.sPassword, 0);

            struLogInfo.wPort = ushort.Parse(serverPort);//设备服务端口号

            if (LoginCallBack == null)
            {
                LoginCallBack = new CHCNetSDK.LOGINRESULTCALLBACK(cbLoginCallBack);//注册回调函数                    
            }
            struLogInfo.cbLoginResult = LoginCallBack;
            struLogInfo.bUseAsynLogin = false; //是否异步登录：0- 否，1- 是 

            DeviceInfo = new CHCNetSDK.NET_DVR_DEVICEINFO_V40();

            //登录设备 Login the device
            m_lUserID = CHCNetSDK.NET_DVR_Login_V40(ref struLogInfo, ref DeviceInfo);
            if (m_lUserID < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_Login_V40 failed, error code= " + iLastErr; //登录失败，输出错误号
                MessageBox.Show(str);
                return;
            }
            else
            {
                //登录成功
                MessageBox.Show("Login Success!");
                //btnLogin.Text = "Logout";
            }

        }
    }

    public bool GetConnectState()
    {
        return m_lUserID > 0;
    }

    public void Disconnect()
    {
        //注销登录 Logout the device
        if (m_lRealHandle >= 0)
        {
            MessageBox.Show("Please stop live view firstly");
            return;
        }

        if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
        {
            iLastErr = CHCNetSDK.NET_DVR_GetLastError();
            str = "NET_DVR_Logout failed, error code= " + iLastErr;
            MessageBox.Show(str);
            return;
        }
        m_lUserID = -1;
    }

    public void PreView(IntPtr widHandle, string channel, string id)
    {
        if (m_lUserID < 0)
        {
            MessageBox.Show("Please login the device firstly");
            return;
        }

        if (m_lRealHandle < 0)
        {
            CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
            lpPreviewInfo.hPlayWnd = widHandle;//预览窗口
            lpPreviewInfo.lChannel = Int16.Parse(channel);//预te览的设备通道
            lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
            lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
            lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
            lpPreviewInfo.dwDisplayBufNum = 1; //播放库播放缓冲区最大缓冲帧数
            lpPreviewInfo.byProtoType = 0;
            lpPreviewInfo.byPreviewMode = 0;

            if (id != "")
            {
                lpPreviewInfo.lChannel = -1;
                byte[] byStreamID = System.Text.Encoding.Default.GetBytes(id);
                lpPreviewInfo.byStreamID = new byte[32];
                byStreamID.CopyTo(lpPreviewInfo.byStreamID, 0);
            }


            if (RealData == null)
            {
                RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数
            }

            IntPtr pUser = new IntPtr();//用户数据

            //打开预览 Start live view 
            m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
            if (m_lRealHandle < 0)
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //预览失败，输出错误号
                MessageBox.Show(str);
                return;
            }
            else
            {
                //预览成功
            }
        }
        return;
    }

    public void StopPreView()
    {
        //停止预览 Stop live view 
        if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
        {
            iLastErr = CHCNetSDK.NET_DVR_GetLastError();
            str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
            MessageBox.Show(str);
            return;
        }
        m_lRealHandle = -1;
    }

    public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
    {
        if (dwBufSize > 0)
        {
            byte[] sData = new byte[dwBufSize];
            Marshal.Copy(pBuffer, sData, 0, (Int32)dwBufSize);

            string str = "实时流数据.ps";
            FileStream fs = new FileStream(str, FileMode.Create);
            int iLen = (int)dwBufSize;
            fs.Write(sData, 0, iLen);
            fs.Close();
        }
    }
}