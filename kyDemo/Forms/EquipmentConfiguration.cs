using kyDemo.Services.client;
using RMCLinkNET;
using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace kyDemo
{
    public partial class EquipmentConfiguration : Form
    {
        private RMCLink rmc;  //液压plc
        public int fd { get; set; }

        public EquipmentConfiguration(Form parent)
        {
            InitializeComponent();
            //设置tab字体横向
            tabControl1.DrawItem += new DrawItemEventHandler(tabControl1_DrawItem);
            // 设置弹出窗口位置在父窗口的中间
            this.StartPosition = FormStartPosition.Manual;
            this.Location = new Point(
            parent.Location.X + (parent.Width - this.Width) / 2,
                parent.Location.Y + (parent.Height - this.Height) / 2
            );
            //if(ControllerClient.Instance.ip_ != "")
            //   txtControlIpAddress.Text = ControllerClient.Instance.ip_;
            //if (ControllerClient.Instance.port_ != "")
            //    txtControlPort.Text = ControllerClient.Instance.port_;
            //txtRMCIpAddress.Text = PLCConnectionManager.Instance.serverIp == "" ? txtRMCIpAddress.Text : PLCConnectionManager.Instance.serverIp;
            //txtCameraIpAddress.Text = CameraConnectionManager.Instance.serverIp == "" ? txtCameraIpAddress.Text : CameraConnectionManager.Instance.serverIp;
            //txtCameraPort.Text = CameraConnectionManager.Instance.serverPort == 0 ? txtCameraPort.Text : CameraConnectionManager.Instance.serverPort.ToString();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string ipAddress = txtControlIpAddress.Text;
            string port = txtControlPort.Text;
            if (ControllerClient.Instance.Connect(ipAddress, port))
            {
                MessageBox.Show("连接成功。");
            } else
            {
                MessageBox.Show("连接失败。");
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            ControllerClient.Instance.DisConnect();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage tabPage = tabControl1.TabPages[e.Index];
            Rectangle tabRect = tabControl1.GetTabRect(e.Index);
            string tabText = tabPage.Text;

            // 设置文本字体和对齐方式
            using (Font font = new Font(e.Font.FontFamily, e.Font.Size, FontStyle.Regular, GraphicsUnit.Point))
            {
                using (StringFormat stringFormat = new StringFormat())
                {
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.FormatFlags = StringFormatFlags.DirectionRightToLeft;

                    // 绘制选项卡文本
                    e.Graphics.DrawString(tabText, font, Brushes.Black, tabRect, stringFormat);
                }
            }
        }

        //PLC连接
        private void button8_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.SetIP(txtRMCIpAddress.Text);
            PLCConnectionManager.Instance.Connect();
        }
        //PLC断开
        private void button7_Click(object sender, EventArgs e)
        {
            PLCConnectionManager.Instance.DisConnect();
        }

        //连接相机
        private async void button4_Click(object sender, EventArgs e)
        {
            if (textBoxIP.Text == "" || textBoxPort.Text == "" ||
                textBoxUserName.Text == "" || textBoxPassword.Text == "")
            {
                MessageBox.Show("Please input IP, Port, User name and Password!");
                return;
            }

            try
            {
                CameraConnectionManager.Instance.SetIPAndPort(textBoxIP.Text, textBoxPort.Text, textBoxUserName.Text, textBoxPassword.Text);
                CameraConnectionManager.Instance.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"连接失败: {ex.Message}");
            }
        }
        //断开相机
        private void button3_Click(object sender, EventArgs e)
        {
            CameraConnectionManager.Instance.Disconnect();
        }

        private void txtCameraPort_TextChanged(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox6.Text;
            if (!int.TryParse(textBox5.Text, out int port))
            {
                MessageBox.Show("请输入有效的端口号。");
                return;
            }
            if (ModbusClient.Instance.Connect(ipAddress, port))
            {
                MessageBox.Show("连接成功。");
            }
            else
            {
                MessageBox.Show("连接失败。");
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ModbusClient.Instance.Disconnect();
            MessageBox.Show("断开连接。");
        }
    }
}
