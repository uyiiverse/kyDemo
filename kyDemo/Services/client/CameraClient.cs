using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class CameraConnectionManager
{
    private static CameraConnectionManager _instance;
    private TcpClient client;
    private NetworkStream stream;
    public string serverIp { get; set; }
    public int serverPort { get; set; }

    private CameraConnectionManager() { }

    public static CameraConnectionManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CameraConnectionManager();
            }
            return _instance;
        }
    }

    public void SetIPAndPort(string ip, int port)
    {
        serverIp = ip;
        serverPort = port;
    }

    public void Connect()
    {
        if (client == null)
        {
            client = new TcpClient(serverIp, serverPort);
            stream = client.GetStream();
        }
        else
        {
            if (!GetConnectState())
            {
                client.Connect(serverIp, serverPort);
            }
        }
    }
    public async Task ConnectAsync()
    {
        if (client == null)
        {
            client = new TcpClient();
            await client.ConnectAsync(serverIp, serverPort);
            stream = client.GetStream();
        }
        else
        {
            if (!GetConnectState())
            {
                await client.ConnectAsync(serverIp, serverPort);
                stream = client.GetStream();
            }
        }
    }
    public bool GetConnectState()
    {
        try
        {
            if (client != null && client.Client != null && client.Client.Connected)
            {
                // 客户端是否连接并不一定表示服务器端还保持连接
                // 所以需要额外的检测
                if (client.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.Client.Receive(buff, SocketFlags.Peek) == 0)
                    {
                        // 连接已断开
                        return false;
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (SocketException)
        {
            return false;
        }
    }
    public (bool,double, double, double) GetPoints()
    {
        if (client == null || stream == null || !GetConnectState())
        {
            return (false, 0, 0, 0);
            //throw new InvalidOperationException("CameraClient is not connected.");
        }
        byte[] requestBytes = Encoding.ASCII.GetBytes("get\n");
        stream.Write(requestBytes, 0, requestBytes.Length);

        byte[] buffer = new byte[256];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        string response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        string[] points = response.Trim().Split(',');
        if (points.Length != 3)
        {
            Console.WriteLine($"解析相机数据时出错。");
            return (false, 0, 0, 0);
        }

        try
        {
            double point1 = (double.Parse(points[0]) * 1000) - 30; //for test
            double point2 = (double.Parse(points[1]) * 1000) - 80;
            double point3 = (double.Parse(points[2]) * 1000);
            return (true,point1, point2, point3);
        }
        catch (FormatException ex)
        {
            Console.WriteLine($"解析相机数据时出错。");
            throw new FormatException("解析相机数据时出错。", ex);
        }
    }

    public void Disconnect()
    {
        if (stream != null)
        {
            stream.Close();
            stream = null;
        }

        if (client != null)
        {
            client.Close();
            client = null;
        }
    }
}