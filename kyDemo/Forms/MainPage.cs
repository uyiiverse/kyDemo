using kyDemo.Forms;
using kyDemo.Services;
using kyDemo.Services.client;
using System;
using System.Threading;
using System.Windows.Forms;
using ReaLTaiizor.Forms;

namespace kyDemo
{
    public partial class MainPage : Form
    {
        private bool isRemote; // 用于记录当前状态，true表示“远程”，false表示“自主”
        private Thread updateThread;
        private volatile bool _isRunning = true;
        private EquipmentConfiguration equipmentConfiguration_;
        private MotionParameters motionParameters_;
        private ProcessSettings processSettings_;
        private TestPage testPage_;
        private IOMonitor ioMonitorPage_;

        public MainPage()
        {
            InitializeComponent();

            // 最大化
            this.WindowState = FormWindowState.Maximized;

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
        private void materialButton1_Click(object sender, EventArgs e)
        {
            isRemote = !isRemote;
            if (isRemote)
            {
                materialButton1.Text = "远程";
                ModbusClient.Instance.StartReadingTask();
            }
            else
            {
                materialButton1.Text = "自主";
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
                            label12.Text = HikVisionConnectionManager.Instance.GetConnectState() ? "已连接" : "未连接";
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
        private void button10_Click(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            TestPage testPage = new TestPage(this);
            testPage.ShowDialog();
        }

        private void button12_Click(object sender, EventArgs e)
        {
            IOMonitor testPage = new IOMonitor(this);
            testPage.ShowDialog();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            HikVisionConnectionManager.Instance.PreView(RealPlayWnd.Handle, textBoxChannel.Text, textBoxID.Text);
        }

        private void button13_Click(object sender, EventArgs e)
        {
            HikVisionConnectionManager.Instance.StopPreView();
        }

        private void uiImageButton1_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void uiImageButton2_Click_1(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnHome_Click(object sender, EventArgs e)
        {
            if (equipmentConfiguration_ == null || equipmentConfiguration_.IsDisposed)
            {
                equipmentConfiguration_ = new EquipmentConfiguration(this);
                equipmentConfiguration_.FormClosed += (_, __) => equipmentConfiguration_ = null;
                equipmentConfiguration_.Show(this);
            }
            else
            {
                equipmentConfiguration_.Activate();   // 已存在则激活
            }
        }

        private void btnChart_Click(object sender, EventArgs e)
        {
            if (motionParameters_ == null || motionParameters_.IsDisposed)
            {
                motionParameters_ = new MotionParameters(this);
                motionParameters_.FormClosed += (_, __) => motionParameters_ = null;
                motionParameters_.Show(this);
            }
            else
            {
                motionParameters_.Activate();   // 已存在则激活
            }
        }
        
        private void btnHistory_Click(object sender, EventArgs e)
        {
            if (processSettings_ == null || processSettings_.IsDisposed)
            {
                processSettings_ = new ProcessSettings(this);
                processSettings_.FormClosed += (_, __) => processSettings_ = null;
                processSettings_.Show(this);
            }
            else
            {
                processSettings_.Activate();   // 已存在则激活
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {

        }

        private void btnParaSet_Click(object sender, EventArgs e)
        {

        }
        
        private void uiImageButton4_Click(object sender, EventArgs e)
        {
            if (testPage_ == null || testPage_.IsDisposed)
            {
                testPage_ = new TestPage(this);
                testPage_.FormClosed += (_, __) => testPage_ = null;
                testPage_.Show(this);
            }
            else
            {
                testPage_.Activate();   // 已存在则激活
            }
        }
        
        private void uiImageButton3_Click(object sender, EventArgs e)
        {
            if (ioMonitorPage_ == null || ioMonitorPage_.IsDisposed)
            {
                ioMonitorPage_ = new IOMonitor(this);
                ioMonitorPage_.FormClosed += (_, __) => ioMonitorPage_ = null;
                ioMonitorPage_.Show(this);
            }
            else
            {
                ioMonitorPage_.Activate();   // 已存在则激活
            }
        }
    }
}
