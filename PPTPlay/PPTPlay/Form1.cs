using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ThoughtWorks;
using ThoughtWorks.QRCode;
using ThoughtWorks.QRCode.Codec;
using ThoughtWorks.QRCode.Codec.Data;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Net.Sockets;
using System.Threading;

namespace PPTPlay
{
    public partial class Form1 : Form
    {
       
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            label5.BackColor = Color.FromArgb(57, 138, 203);
            label6.BackColor = Color.FromArgb(57, 138, 203);



            //初始化txtOrange的颜色
            txtOrange.BackColor = Color.Orange; 
            //查找本机的IP端口写到label2中
            #region
            string AddressIP = string.Empty;
            foreach (IPAddress _IPAddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (_IPAddress.AddressFamily.ToString() == "InterNetwork")
                {
                    AddressIP = _IPAddress.ToString();
                }
            }
            IPAddress localIP = IPAddress.Parse(AddressIP);
            IPEndPoint iep = new IPEndPoint(localIP, 11751);         
            label2.Text = iep.ToString();
            #endregion
            //扫描本机的IP端口并生成二维码，放到PictureBox1中
            #region
            ThoughtWorks.QRCode.Codec.QRCodeEncoder encoder = new QRCodeEncoder();
            encoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.ALPHA_NUMERIC;//编码方法
            encoder.QRCodeScale = 4;//大小
            encoder.QRCodeVersion = 4;//版本
            encoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.M;
            String qrdata = label2.Text; ;
            System.Drawing.Bitmap bp = encoder.Encode(qrdata.ToString(), Encoding.GetEncoding("GB2312"));
            Image image = bp;
            Object oMissing = System.Reflection.Missing.Value;
            pictureBox1.Image = bp;
            #endregion
            //开启一个线程，控制播放PPT
            #region
            Thread backthread = new Thread(sever);
            // 使线程成为一个后台线程
            //backthread.IsBackground = true;
            // 通过Start方法启动线程
            backthread.Start();
           
            #endregion
        }
        #region 模拟键盘
        [DllImport("USER32.DLL")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);  //导入寻找windows窗体的方法
        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);  //导入为windows窗体设置焦点的方法
        [DllImport("USER32.DLL")]
        public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);  //导入模拟键盘的方法
        #endregion
        //编写交互数据，进行接受命令，上一页，下一页
        #region
        public void sever()
        {
            try
            {          
            int recv;
            byte[] data = new byte[1024];
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 11751);
            Socket network = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            network.Bind(ipep);
            IPEndPoint send = new IPEndPoint(IPAddress.Any, 0);
            EndPoint Remote = (EndPoint)(send);
            // string ip = Remote.ToString();
            recv = network.ReceiveFrom(data, ref Remote);
            network.SendTo(data, data.Length, SocketFlags.None, Remote);
            string ip = Remote.ToString();
            string[] sp = ip.Split(':');
            while (true)
            {
                data = new byte[1024];
                recv = network.ReceiveFrom(data, ref Remote);
                string shuju = Encoding.ASCII.GetString(data, 0, recv);
                switch (shuju)
                {
                    case "next": Uppage();


                        textBox1.Invoke(new Action(
                    delegate
                    {

                        textBox1.Text += "IP地址为：" + sp[0] + "," + "端口号为：" + sp[1] + "下一页" + "\r\n";
                        txtGreen.BackColor = Color.Green;
                        txtOrange.BackColor = Color.White;
                        txtRed.BackColor = Color.White;


                    }
                     ));
                        break;

                    case "pre": Nextpage();
                        textBox1.Invoke(new Action(
                  delegate
                  {
                      textBox1.Text += "IP地址为：" + sp[0] + "," + "端口号为：" + sp[1] + "上一页" + "\r\n";
                      txtRed.BackColor = Color.Green;
                      txtOrange.BackColor = Color.White;
                      txtGreen.BackColor = Color.White;

                  }
                   ));
                        break;

                    default: break;
                }
            }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        #endregion
        //模拟上键操作
        public void Uppage()
        {
            //按住上键
            keybd_event(0x28, 0, 0, 0);
            //松开上键
            keybd_event(0x28, 0, 2, 0);
            //MessageBox.Show("你说入的的是上键");
        }
        //模拟下键操作
        public void Nextpage()
        {
            //按住下键
            keybd_event(0x26, 0, 0, 0);
            //松开下键
            keybd_event(0x26, 0, 2, 0);
            //MessageBox.Show("你说入的的是下键");
        }
        //编写窗体关闭事件
        private void label5_Click(object sender, EventArgs e)
        {
            //Form1.ActiveForm.Close(); 只是关闭当前窗口，若不是主窗体的话，是无法退出程序的，另外若有托管线程（非主线程），也无法干净地退出；
            //Application.Exit();强制所有消息中止，退出所有的窗体，但是若有托管线程（非主线程），也无法干净地退出；
            System.Environment.Exit(-1); //这是最彻底的退出方式，不管什么线程都被强制退出，把程序结束的很干净。  
        }
        //编写窗体最小化事件
        private void label6_Click(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Normal)
            {
                this.WindowState = FormWindowState.Minimized;   //546           
                
            }
            else if (this.WindowState == FormWindowState.Minimized)
            {
                this.Show();
            }
        }

        //设置窗体拖动事件
        #region
        Point mouseOff;//鼠标移动位置变量
        bool leftFlag;//标记是否为左键
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mouseOff = new Point(-e.X, -e.Y); //得到变量的值
                leftFlag = true;                  //点击左键按下时标注为true;
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                Point mouseSet = Control.MousePosition;
                mouseSet.Offset(mouseOff.X, mouseOff.Y);  //设置移动后的位置
                Location = mouseSet;
            }
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (leftFlag)
            {
                leftFlag = false;//释放鼠标后标注为false;
            }
        }
        #endregion

    }

    
}

