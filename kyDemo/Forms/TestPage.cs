using kyDemo.Services;
using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace kyDemo.Forms
{
    public partial class TestPage : Form
    {
        private bool isRemote; // 用于记录当前状态，true表示“远程”，false表示“自主”
        private Thread updateThread;
        private volatile bool _isRunning = true;
        public TestPage(Form parent)
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
                            double[] currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
                            double[] currentPositionACS = ControllerClient.Instance.GetPositionByLength(currentLengths, 0);
                            double[] currentPositionMCS = ControllerClient.Instance.GetPositionByLength(currentLengths, 1);
                            label1.Text = currentLengths[0].ToString("F3");
                            label2.Text = currentLengths[1].ToString("F3");
                            label3.Text = currentLengths[2].ToString("F3");
                            label4.Text = currentLengths[3].ToString("F3");
                            label5.Text = currentLengths[4].ToString("F3");
                            label6.Text = currentPositionACS[0].ToString("F3");
                            label7.Text = currentPositionACS[1].ToString("F3");
                            label8.Text = currentPositionACS[2].ToString("F3");
                            label9.Text = currentPositionACS[3].ToString("F3");
                            label10.Text = currentPositionACS[4].ToString("F4");
                            label11.Text = currentPositionMCS[0].ToString("F4");
                            label12.Text = currentPositionMCS[1].ToString("F4");
                            label13.Text = currentPositionMCS[2].ToString("F4");
                            label14.Text = currentPositionMCS[3].ToString("F4");
                            label15.Text = currentPositionMCS[4].ToString("F4");
                            label26.Text = currentPositionMCS[5].ToString("F4");
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

        private void button2_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            string formattedDateTime = now.ToString("yyyy-MM-dd HH:mm:ss");
            Console.WriteLine(formattedDateTime + "指令点击瞬间的时间");
            double cameraX = 0;   //前端页面输入的门型指令高度
            double cameraY = 0;
            double cameraZ = 0;
            double cameraA = 0;   //前端页面输入的门型指令高度
            double cameraB = 0;
            double cameraC = 0;
            double height = 0;
            if (double.TryParse(textBox1.Text, out cameraX) && double.TryParse(textBox2.Text, out cameraY)
                 && double.TryParse(textBox3.Text, out cameraZ) && double.TryParse(textBox4.Text, out height))
            {
                    TreatmentProcess.InsertGateInstruction(cameraX, cameraY, cameraZ, height);
            }
            else
            {
                Console.WriteLine("输入的门型参数有误.");
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double shuffle_instruction_length = 0;
            double shuffle_instruction_width = 0;
            double shuffle_instruction_segment = 0;
            double cameraX = 0;
            double cameraY = 0;
            double cameraZ = 0;
            if (double.TryParse(textBox7.Text, out shuffle_instruction_length) && double.TryParse(textBox6.Text, out shuffle_instruction_width)
                 && double.TryParse(textBox5.Text, out shuffle_instruction_segment) && double.TryParse(textBox1.Text, out cameraX) 
                 && double.TryParse(textBox2.Text, out cameraY) && double.TryParse(textBox3.Text, out cameraZ))
            {
                TreatmentProcess.InsertShuffleInstruction(cameraX, cameraY, cameraZ, shuffle_instruction_length, shuffle_instruction_width, shuffle_instruction_segment);
            }
            else
            {
                Console.WriteLine("输入的推散参数有误.");
            }
        }

        private void label16_Click(object sender, EventArgs e)
        {

        }

        private void label26_Click(object sender, EventArgs e)
        {

        }

        private void button3_Click(object sender, EventArgs e)
        {
            (bool result, double cameraX, double cameraY, double cameraZ) = CameraConnectionManager.Instance.GetPoints();
            textBox1.Text = cameraX.ToString("F3");
            textBox2.Text = cameraY.ToString("F3");
            textBox3.Text = cameraZ.ToString("F3");
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
