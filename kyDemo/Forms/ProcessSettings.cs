using kyDemo.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace kyDemo
{
    public partial class ProcessSettings : Form
    {
        public ProcessSettings(Form parent)
        {
            InitializeComponent();
            // 设置弹出窗口位置在父窗口的中间
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
            parent.Location.X + (parent.Width - this.Width) / 2,
                parent.Location.Y + (parent.Height - this.Height) / 2
            );
            label2.BackgroundImageLayout = ImageLayout.Stretch;
            ParamModel.LoadUserData();
            textBox1.Text = ParamModel.Instance.gate_instruction_height.ToString();
            textBox2.Text = ParamModel.Instance.shuffle_instruction_length.ToString();
            textBox3.Text = ParamModel.Instance.shuffle_instruction_width.ToString();
            textBox4.Text = ParamModel.Instance.shuffle_instruction_segments.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //获取相机的一个点
            //(bool result, double x, double y, double z)  = CameraConnectionManager.Instance.GetPoints();
            //if(result)
            //{
            //    Console.WriteLine("获取成功.");
            //}
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double gate_instruction_height = 0;   //前端页面输入的门型指令高度
            if (double.TryParse(textBox1.Text, out gate_instruction_height))
            {
                if (TreatmentProcess.GetMode() == TreatmentProcess.Mode.insert)
                {
                    //(bool result, double cameraX, double cameraY, double cameraZ) = CameraConnectionManager.Instance.GetPoints();
                    //if (result)
                    //{
                    //    TreatmentProcess.InsertGateInstruction(cameraX, cameraY, cameraZ, gate_instruction_height);
                    //}
                }
                else
                {
                    Console.WriteLine("当前不在插入模式.");
                }
            }
            else
            {
                Console.WriteLine("输入的门型高度有误."); 
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            double gate_instruction_height = 0;   //前端页面输入的门型指令高度
            double shuffle_instruction_length = 0;
            double shuffle_instruction_width = 0;
            int shuffle_instruction_segments = 0;
            if (double.TryParse(textBox1.Text, out gate_instruction_height)
                && double.TryParse(textBox2.Text, out shuffle_instruction_length)
                && double.TryParse(textBox3.Text, out shuffle_instruction_width)
                && int.TryParse(textBox4.Text, out shuffle_instruction_segments))
            {
                if (TreatmentProcess.GetMode() == TreatmentProcess.Mode.auto)
                {
                    ParamModel.LoadUserData();
                    ParamModel.Instance.gate_instruction_height = gate_instruction_height;
                    ParamModel.Instance.shuffle_instruction_length = shuffle_instruction_length;
                    ParamModel.Instance.shuffle_instruction_width = shuffle_instruction_width;
                    ParamModel.Instance.shuffle_instruction_segments = shuffle_instruction_segments;
                    ParamModel.SaveUserData();
                    MessageBox.Show("保存成功。");
                }
                else
                {
                    Console.WriteLine("当前不在插入模式.");
                }
            }
            else
            {
                Console.WriteLine("输入的参数有误.");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            TreatmentProcess.SwitchMode(TreatmentProcess.Mode.auto);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            TreatmentProcess.SwitchMode(TreatmentProcess.Mode.insert);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.GoHome();
        }
        private void button8_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.GoReady();
        }
        private void button7_Click(object sender, EventArgs e)
        {

        }
    }
}
