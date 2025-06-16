using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;
using Newtonsoft.Json;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kyDemo.Forms
{
    public partial class MotionParameters : Form
    {
        public MotionParameters(Form parent)
        {
            InitializeComponent();
            // 设置弹出窗口位置在父窗口的中间
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
            parent.Location.X + (parent.Width - this.Width) / 2,
                parent.Location.Y + (parent.Height - this.Height) / 2
            );
            ParamModel.LoadUserData();
            textBox1.Text = ParamModel.Instance.line_vel.ToString();
            textBox2.Text = ParamModel.Instance.line_acc.ToString();
            textBox3.Text = ParamModel.Instance.line_dec.ToString();
            textBox4.Text = ParamModel.Instance.line_pl.ToString();
            textBox5.Text = ParamModel.Instance.step.ToString();
            textBox6.Text = ParamModel.Instance.L1_zero.ToString();
            textBox7.Text = ParamModel.Instance.L2_zero.ToString();
            textBox8.Text = ParamModel.Instance.L3_zero.ToString();
            textBox9.Text = ParamModel.Instance.L4_zero.ToString();
            textBox10.Text = ParamModel.Instance.L5_zero.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            double vel = 0;   //前端页面输入的门型指令高度
            double acc = 0;
            double dec = 0;
            int pl = 0;
            int step = 0;
            double L1_zero = 0;
            double L2_zero = 0;
            double L3_zero = 0;
            double L4_zero = 0;
            double L5_zero = 0;
            if (double.TryParse(textBox1.Text, out vel)
                && double.TryParse(textBox2.Text, out acc)
                && double.TryParse(textBox3.Text, out dec)
                && int.TryParse(textBox4.Text, out pl)
                && int.TryParse(textBox5.Text, out step)
                && double.TryParse(textBox6.Text, out L1_zero)
                && double.TryParse(textBox7.Text, out L2_zero)
                && double.TryParse(textBox8.Text, out L3_zero)
                && double.TryParse(textBox9.Text, out L4_zero)
                && double.TryParse(textBox10.Text, out L5_zero))
            {
                ParamModel.Instance.line_vel = vel;
                ParamModel.Instance.line_acc = acc;
                ParamModel.Instance.line_dec = dec;
                ParamModel.Instance.line_pl = pl;
                ParamModel.Instance.step = step;
                ParamModel.Instance.L1_zero = L1_zero;
                ParamModel.Instance.L2_zero = L2_zero;
                ParamModel.Instance.L3_zero = L3_zero;
                ParamModel.Instance.L4_zero = L4_zero;
                ParamModel.Instance.L5_zero = L5_zero;
                ParamModel.SaveUserData();
                MessageBox.Show("保存成功。");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            double[] currentLengths = PLCConnectionManager.Instance.GetCurrentLength();
            int coordinate = 1; //直角坐标
            double[] currentPosition = ControllerClient.Instance.GetPositionByLength(currentLengths, coordinate);
            ParamModel.Instance.startPosition  = currentPosition;
            ParamModel.SaveUserData();
            MessageBox.Show("标定成功。");
        }
    }
}
