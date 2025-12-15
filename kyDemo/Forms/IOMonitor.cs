using kyDemo.Services.client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kyDemo.Forms
{
    public partial class IOMonitor : Form
    {

        private bool isRemote; // 用于记录当前状态，true表示“远程”，false表示“自主”
        private Thread updateThread;
        private volatile bool _isRunning = true;

        public IOMonitor(Form parent)
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
            parent.Location.X + (parent.Width - this.Width) / 2,
                parent.Location.Y + (parent.Height - this.Height) / 2
            );
            updateThread = new Thread(UpdateLabel);
            updateThread.IsBackground = true; // 设置为后台线程
            updateThread.Start();
        }

        private void UpdateLabel()
        {
            int count = 0;
            while (_isRunning)
            {
                if (this.IsHandleCreated && !this.IsDisposed)
                {
                    try
                    {
                        // 更新Label的数值
                        this.Invoke((MethodInvoker)delegate
                        {
                            if (ModbusClient.Instance.IsConnected)
                            {
                                // modbus
                                int _startAddress = 150;
                                int YeyazhanDianjiQitingControlValuesIndex = 177 - _startAddress;
                                int EmergencyStopControlValuesIndex = 179 - _startAddress;
                                int PosuiControlValuesIndex = 181 - _startAddress;
                                int LabaControlValuesIndex = 183 - _startAddress;

                                int huizhuanControlValuesIndex = 161 - _startAddress;
                                int dabiControlValuesIndex = 163 - _startAddress;
                                int erbiControlValuesIndex = 165 - _startAddress;
                                int sanbiControlValuesIndex = 167 - _startAddress;

                                labelmobus1.Text = ModbusClient.Instance._controlValues[YeyazhanDianjiQitingControlValuesIndex].ToString();
                                labelmobus2.Text = ModbusClient.Instance._controlValues[EmergencyStopControlValuesIndex].ToString();
                                labelmobus3.Text = ModbusClient.Instance._controlValues[PosuiControlValuesIndex].ToString();
                                labelmobus4.Text = ModbusClient.Instance._controlValues[LabaControlValuesIndex].ToString();
                                labelmobus5.Text = ModbusClient.Instance._controlValues[huizhuanControlValuesIndex].ToString();
                                labelmobus6.Text = ModbusClient.Instance._controlValues[dabiControlValuesIndex].ToString();
                                labelmobus7.Text = ModbusClient.Instance._controlValues[erbiControlValuesIndex].ToString();
                                labelmobus8.Text = ModbusClient.Instance._controlValues[sanbiControlValuesIndex].ToString();
                            }

                            if (PLCConnectionManager.Instance.GetConnectState())
                            {
                                plc1.Text = PLCConnectionManager.Instance.GetL1().ToString();
                                plc2.Text = PLCConnectionManager.Instance.GetL3().ToString();
                                plc3.Text = PLCConnectionManager.Instance.GetL4().ToString();
                                plc4.Text = PLCConnectionManager.Instance.GetL5().ToString();

                                plc6.Text = PLCConnectionManager.Instance.GetMotorStart().ToString();
                                plc7.Text = PLCConnectionManager.Instance.GetEmergencyStop().ToString();
                                plc8.Text = PLCConnectionManager.Instance.GetPoSui().ToString();
                                plc9.Text = PLCConnectionManager.Instance.GetLaBa().ToString();

                                plc10.Text = PLCConnectionManager.Instance.GetHuiZhuan().ToString();
                                plc11.Text = PLCConnectionManager.Instance.GetDaBi().ToString();
                                plc12.Text = PLCConnectionManager.Instance.GeErBi().ToString();
                                plc13.Text = PLCConnectionManager.Instance.GetSanBi().ToString();

                                plc14.Text = PLCConnectionManager.Instance.GetHuiZhuanVoltageOutput().ToString();
                                plc15.Text = PLCConnectionManager.Instance.GetDaBiVoltageOutput().ToString();
                                plc16.Text = PLCConnectionManager.Instance.GetErBiVoltageOutput().ToString();
                                plc17.Text = PLCConnectionManager.Instance.GetSanBiVoltageOutput().ToString();
                            }
                            
                        });
                        count++;
                        Thread.Sleep(1000); // 每秒更新一次
                    }
                    catch (ObjectDisposedException)
                    {
                        continue;
                    }
                    catch (InvalidOperationException)
                    {
                        continue;
                    }
                }
            }
        }
    }
}
