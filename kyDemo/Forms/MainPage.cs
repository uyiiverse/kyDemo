using kyDemo.Forms;
using kyDemo.Services;
using kyDemo.Services.client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace kyDemo
{
    public partial class MainPage : Form
    {
        private bool isRemote; // 用于记录当前状态，true表示“远程”，false表示“自主”
        private Thread updateThread;
        private volatile bool _isRunning = true;
        public MainPage()
        {
            InitializeComponent();
            isRemote = true; // 初始状态为“远程”

            updateThread = new Thread(UpdateLabel);
            updateThread.IsBackground = true; // 设置为后台线程
            updateThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            EquipmentConfiguration equipmentConfiguration = new EquipmentConfiguration(this);
            equipmentConfiguration.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            isRemote = !isRemote;
            if (isRemote)
            {
                button1.Text = "远程";
                ModbusClient.Instance.StartReadingTask();
            }
            else
            {
                button1.Text = "自主";
                ModbusClient.Instance.StopReadingTask();
            }
            Console.WriteLine($"当前状态: {(isRemote ? "远程" : "自主")}");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            ProcessSettings processSettings = new ProcessSettings(this);
            processSettings.ShowDialog();
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
                            label15.Text = PLCConnectionManager.Instance.GetOilPress().ToString();
                            label3.Text = PLCConnectionManager.Instance.GetOilTemperature().ToString();
                            label5.Text = PLCConnectionManager.Instance.GetOilLevel().ToString();
                            label7.Text = PLCConnectionManager.Instance.GetMotorCurrent().ToString();
                            label9.Text = PLCConnectionManager.Instance.GetMotorVoltage().ToString(); ;
                            label10.Text = ControllerClient.Instance.GetConnectState() ? "已连接" : "未连接";
                            label11.Text = PLCConnectionManager.Instance.GetConnectState() ? "已连接" : "未连接";
                            label12.Text = CameraConnectionManager.Instance.GetConnectState() ? "已连接" : "未连接";
                        });
                        count++;
                        Thread.Sleep(500); // 每秒更新一次
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

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (updateThread != null && updateThread.IsAlive)
            {
                updateThread.Abort();
            }
            base.OnFormClosing(e);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.EmergencyStop(1);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.EmergencyStop(0);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MotionParameters motionParameters = new MotionParameters(this);
            motionParameters.ShowDialog();
        }

        private async void button8_Click(object sender, EventArgs e)
        {
            ParamModel.LoadUserData();
            bool res = await TreatmentProcess.AutomaticProcessing(ParamModel.Instance.gate_instruction_height
                        , ParamModel.Instance.shuffle_instruction_length
                        , ParamModel.Instance.shuffle_instruction_width
                        , ParamModel.Instance.shuffle_instruction_segments);
        }

        private void button9_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.StopMove();
            TreatmentProcess.running = false;
        }

        private void button11_Click(object sender, EventArgs e)
        {
            TestPage testPage = new TestPage(this);
            testPage.ShowDialog();
        }

        private void button10_Click(object sender, EventArgs e)
        {

        }
    }
}
