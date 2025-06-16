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

        // 私有构造函数，防止外部实例化
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
                        byte slaveId = 1;
                        ushort startAddress = 150;
                        ushort numRegisters = 50;
                        ushort[] controlValues = _modbusMaster.ReadHoldingRegisters(slaveId, startAddress, numRegisters);

                        // 处理寄存器 40179 (急停状态位)
                       // int EmergencyStopControlValuesIndex = 179 - startAddress;
                       /* ushort register40001 = controlValues[EmergencyStopControlValuesIndex];
                        Console.WriteLine("寄存器40179 (开关状态)：");
                        for (int bit = 15; bit >= 0; bit--)
                        {
                            bool bitValue = (register40001 & (1 << bit)) != 0;
                            Console.WriteLine($"Bit {bit}: {bitValue}");
                        }*/

                        //PLCConnectionManager.Instance.WriteOneDataToRMCRegisterAbsolute(controlValues[EmergencyStopControlValuesIndex], 61); ; // 急停
                        int JingxiaWuliBendiKaiguanlControlValuesIndex = 157 - startAddress;
                        int YeyazhanDianjiQitingControlValuesIndex = 177 - startAddress;
                        int EmergencyStopControlValuesIndex = 179 - startAddress;
                        int PosuiControlValuesIndex = 181 - startAddress;
                        int LabaControlValuesIndex = 183 - startAddress;

                        int chandouControlValuesIndex = 167 - startAddress;
                        int huizhuanControlValuesIndex = 161 - startAddress;
                        int douganControlValuesIndex = 165 - startAddress;
                        int dabiControlValuesIndex = 163 - startAddress;
                        int chandouSignControlValuesIndex = 175 - startAddress;
                        int huizhuanSignControlValuesIndex = 169 - startAddress;
                        int douganSignControlValuesIndex = 173 - startAddress;
                        int dabiSignControlValuesIndex = 171 - startAddress;
                      
                        Console.WriteLine("控制值：");
                        Console.WriteLine($"井下物理本地开关：{ConvertToSignedInt(controlValues[JingxiaWuliBendiKaiguanlControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegisterAbsolute(controlValues[JingxiaWuliBendiKaiguanlControlValuesIndex], 78);
                        Console.WriteLine($"液压站电机启停：{ConvertToSignedInt(controlValues[YeyazhanDianjiQitingControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[huizhuanControlValuesIndex], 88);
                        Console.WriteLine($"急停：{ConvertToSignedInt(controlValues[EmergencyStopControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[EmergencyStopControlValuesIndex], 89);
                        Console.WriteLine($"破碎：{ConvertToSignedInt(controlValues[PosuiControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[PosuiControlValuesIndex], 90);
                        Console.WriteLine($"喇叭：{ConvertToSignedInt(controlValues[LabaControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[PosuiControlValuesIndex], 91);


                        Console.WriteLine($"铲斗控制值：{ConvertToSignedInt(controlValues[chandouControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegisterAbsolute(controlValues[chandouControlValuesIndex],83);
                        Console.WriteLine($"回转控制值：{ConvertToSignedInt(controlValues[huizhuanControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[huizhuanControlValuesIndex], 80);
                        Console.WriteLine($"斗杆控制值：{ConvertToSignedInt(controlValues[douganControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[douganControlValuesIndex], 82);
                        Console.WriteLine($"大臂上升下降控制值：{ConvertToSignedInt(controlValues[dabiControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[dabiControlValuesIndex], 81);
                        Console.WriteLine($"铲斗正负：{ConvertToSignedInt(controlValues[chandouSignControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[chandouSignControlValuesIndex], 87);
                        Console.WriteLine($"回转正负：{ConvertToSignedInt(controlValues[huizhuanSignControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[huizhuanSignControlValuesIndex], 84);
                        Console.WriteLine($"斗杆正负：{ConvertToSignedInt(controlValues[douganSignControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[douganSignControlValuesIndex], 86);
                        Console.WriteLine($"大臂正负：{ConvertToSignedInt(controlValues[dabiSignControlValuesIndex])}");
                        PLCConnectionManager.Instance.WriteOneDataToRMCRegister(controlValues[dabiSignControlValuesIndex], 85);
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
