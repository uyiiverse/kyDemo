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
                                int JingxiaWuliBendiKaiguanlControlValuesIndex = 157 - _startAddress;
                                int YeyazhanDianjiQitingControlValuesIndex = 177 - _startAddress;
                                int EmergencyStopControlValuesIndex = 179 - _startAddress;
                                int PosuiControlValuesIndex = 181 - _startAddress;
                                int LabaControlValuesIndex = 183 - _startAddress;
                                int chandouControlValuesIndex = 167 - _startAddress;
                                int huizhuanControlValuesIndex = 161 - _startAddress;
                                int douganControlValuesIndex = 165 - _startAddress;
                                int dabiControlValuesIndex = 163 - _startAddress;
                                int chandouSignControlValuesIndex = 175 - _startAddress;
                                int huizhuanSignControlValuesIndex = 169 - _startAddress;
                                int douganSignControlValuesIndex = 173 - _startAddress;
                                int dabiSignControlValuesIndex = 171 - _startAddress;
                                labelmobus1.Text = ModbusClient.Instance._controlValues[JingxiaWuliBendiKaiguanlControlValuesIndex].ToString();
                                labelmobus2.Text = ModbusClient.Instance._controlValues[YeyazhanDianjiQitingControlValuesIndex].ToString();
                                labelmobus3.Text = ModbusClient.Instance._controlValues[EmergencyStopControlValuesIndex].ToString();
                                labelmobus4.Text = ModbusClient.Instance._controlValues[PosuiControlValuesIndex].ToString();
                                labelmobus5.Text = ModbusClient.Instance._controlValues[LabaControlValuesIndex].ToString();
                                labelmobus6.Text = ModbusClient.Instance._controlValues[chandouControlValuesIndex].ToString();
                                labelmobus7.Text = ModbusClient.Instance._controlValues[huizhuanControlValuesIndex].ToString();
                                labelmobus8.Text = ModbusClient.Instance._controlValues[douganControlValuesIndex].ToString();
                                labelmobus9.Text = ModbusClient.Instance._controlValues[dabiControlValuesIndex].ToString();
                                labelmobus10.Text = ModbusClient.Instance._controlValues[chandouSignControlValuesIndex].ToString();
                                labelmobus11.Text = ModbusClient.Instance._controlValues[huizhuanSignControlValuesIndex].ToString();
                                labelmobus12.Text = ModbusClient.Instance._controlValues[douganSignControlValuesIndex].ToString();
                                labelmobus13.Text = ModbusClient.Instance._controlValues[dabiSignControlValuesIndex].ToString();
                            }

                            if (PLCConnectionManager.Instance.GetConnectState())
                            {
                                plc1.Text = PLCConnectionManager.Instance.GetL1().ToString();
                                plc2.Text = PLCConnectionManager.Instance.GetL3().ToString();
                                plc3.Text = PLCConnectionManager.Instance.GetL4().ToString();
                                plc4.Text = PLCConnectionManager.Instance.GetL5().ToString();
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
