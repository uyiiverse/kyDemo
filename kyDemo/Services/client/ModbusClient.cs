using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using NModbus;

namespace kyDemo.Services.client
{
    internal class ModbusClient
    {
        private static ModbusClient _instance;
        private static readonly object _lock = new object();

        private TcpClient _tcpClient;
        private IModbusMaster _modbusMaster;
        private CancellationTokenSource _cancellationTokenSource;
        private string _ipAddress;
        private int _port;
        private bool _connected = false;
        private byte _slaveId = 1;
        private ushort _startAddress = 150;
        private ushort _numRegisters = 50;
        public ushort[] _controlValues;

        public bool IsConnected => _connected;
        private ModbusClient() { }

        // 获取 ModbusClient 的唯一实例（单例模式）
        public static ModbusClient Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new ModbusClient();
                        }
                    }
                }
                return _instance;
            }
        }

        // 连接到 Modbus 服务端
        public bool Connect(string ipAddress, int port = 502)
        {
            if (_connected)
            {
                Console.WriteLine("Already connected.");
                return true;
            }

            try
            {
                _ipAddress = ipAddress;
                _port = port;
                _tcpClient = new TcpClient(ipAddress, port);

                var factory = new ModbusFactory();
                _modbusMaster = factory.CreateMaster(_tcpClient);
                _connected = true;

                Console.WriteLine("Connected to Modbus server.");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to connect to Modbus server: {ex.Message}");
                return false;
            }
        }

        public void SendModbusDataToRMC()
        {
            int YeyazhanDianjiQitingControlValuesIndex = 177 - _startAddress;
            int EmergencyStopControlValuesIndex = 179 - _startAddress;
            int PosuiControlValuesIndex = 181 - _startAddress;
            int LabaControlValuesIndex = 183 - _startAddress;

            int huizhuanControlValuesIndex = 161 - _startAddress;
            int dabiControlValuesIndex = 163 - _startAddress;
            int erbiControlValuesIndex = 165 - _startAddress;
            int sanbiControlValuesIndex = 167 - _startAddress;

            Console.WriteLine("控制值：");

            Console.WriteLine($"液压站电机启停：{ConvertToSignedInt(_controlValues[YeyazhanDianjiQitingControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[huizhuanControlValuesIndex], 88);

            Console.WriteLine($"急停：{ConvertToSignedInt(_controlValues[EmergencyStopControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[EmergencyStopControlValuesIndex], 89);

            Console.WriteLine($"破碎：{ConvertToSignedInt(_controlValues[PosuiControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[PosuiControlValuesIndex], 90);

            Console.WriteLine($"喇叭：{ConvertToSignedInt(_controlValues[LabaControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[PosuiControlValuesIndex], 91);

            Console.WriteLine($"回转控制值：{ConvertToSignedInt(_controlValues[huizhuanControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[huizhuanControlValuesIndex], 80);

            Console.WriteLine($"大臂控制值：{ConvertToSignedInt(_controlValues[dabiControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[dabiControlValuesIndex], 81);

            Console.WriteLine($"二臂控制值：{ConvertToSignedInt(_controlValues[erbiControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegister(_controlValues[erbiControlValuesIndex], 82);

            Console.WriteLine($"三臂控制值：{ConvertToSignedInt(_controlValues[sanbiControlValuesIndex])}");
            PLCConnectionManager.Instance.WriteOneDataToRMCRegisterAbsolute(_controlValues[sanbiControlValuesIndex], 83);
        }
        // 开启一个任务来定时读取数据
        public void StartReading(CancellationToken cancellationToken)
        {
            if (!_connected)
            {
                Console.WriteLine("Not connected to Modbus server.");
                return;
            }

            Task.Run(() =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        // 读取40001至40005的控制值
                        _controlValues = _modbusMaster.ReadHoldingRegisters(_slaveId, _startAddress, _numRegisters);
                        // 将数据发送给RMC
                        SendModbusDataToRMC();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading data: {ex.Message}");
                    }

                    // 每隔一段时间读取一次
                    Task.Delay(500).Wait();
                }
            }, cancellationToken);
        }

        // 断开连接并停止读取
        public void Disconnect()
        {
            if (_connected)
            {
                _cancellationTokenSource?.Cancel();
                _tcpClient.Close();
                _connected = false;
                Console.WriteLine("Disconnected from Modbus server.");
            }
        }

        // 将 Modbus 寄存器值转换为有符号整数
        private int ConvertToSignedInt(ushort value)
        {
            if (value > 32767)
            {
                return value - 65536;
            }
            return value;
        }

        // 开启读取任务
        public void StartReadingTask()
        {
            if (!_connected)
            {
                Console.WriteLine("Not connected. Cannot start reading task.");
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            StartReading(_cancellationTokenSource.Token);
            Console.WriteLine("Started reading task.");
        }

        // 停止读取任务
        public void StopReadingTask()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                Console.WriteLine("Stopped reading task.");
            }
        }

    }
}
