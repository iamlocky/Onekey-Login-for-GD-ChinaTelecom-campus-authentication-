using Microsoft.Win32;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace signin
{
    
    public partial class Form1 : Form
    {
        
        static string authData = "authData.txt";
        static string authDataStr;
        static string[] authDataStrAll=new string[5];
        static int mode = 2;
        static int hour;
        static int min;
        static int sec;
        static string urltmp = "http://219.136.125.139/portalReceiveAction.do?wlanacname=gzucm&wlanuserip=";
        static string cyber = "http://cy-ber.cn";
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)//-------button
        {
            string url;
            if (mode == 1)
            {
                url = textURL.Text;
            }
            else
            {
                url = urltmp + ipAddress.Text;
                textURL.Text = url;
            }
            try
            {
                
                webBrowser1.Navigate(url);
                saveData();
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message.ToString(), "错误", MessageBoxButtons.OK);
            }

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public void check()
        {
            while (webBrowser1.ReadyState != WebBrowserReadyState.Complete)
            {
                return;
            }

        }
        HtmlDocument doc;
        private void checkSucceed()
        {

            Thread.Sleep(1000);
            string A;
            string html = doc.Body.OuterHtml;
            if (html.Contains("您已经登录"))
            {
                A = "<html>\r\n<head>\r\n</head>\r\n<body>\r\n<p> 登录成功！</p>\r\n</body>\r\n</html>";
            }
            else
            {
                A = "<html>\r\n<head>\r\n</head>\r\n<body>\r\n<p> 登录失败，请重新点击一键登录！</p>\r\n</body>\r\n</html>";
            }
            webBrowser1.DocumentText = A;
        }


        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
             doc = webBrowser1.Document;
            doc.GetElementById("useridtemp").InnerText = idNum.Text;
            doc.GetElementById("passwd").InnerText = password.Text;
            doc.GetElementById("passwd").Focus();
            SendKeys.SendWait("{ENTER}");
            Thread th1 = new Thread(new ThreadStart(checkSucceed));
            th1.Start();
        }

        

        private void Form1_Load(object sender, EventArgs e)//-----------------------
        {

            webBrowser1.ScriptErrorsSuppressed = true;//禁止弹出js错误

            IPHostEntry ipe = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ip;
            string iptext;
            ip = ipe.AddressList[1];
            iptext = ip.ToString();
            if (isExists(iptext))
            {
                ip = ipe.AddressList[2];
                iptext = ip.ToString();
            }

            ipAddress.Text = iptext;
            loadData();
        }


        bool isExists(string str)//检测是否ipv6
        {
            return Regex.Matches(str, "[a-zA-Z]").Count > 0;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            textURL.Enabled = false;
            panel1.Enabled = true;
            mode = 2;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            textURL.Enabled = true;
            panel1.Enabled = false;
            mode = 1;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {

            DateTime currentTime = new System.DateTime();
            currentTime = System.DateTime.Now;
            hour = currentTime.Hour;
            min = currentTime.Minute;
            sec = currentTime.Second;
            time.Text = "系统时间：" + hour.ToString("00") + ":" + min.ToString("00") + ":" + sec.ToString("00");
            if (textBoxHour.Text != "" && textBoxMin.Text != "")
            {
                if (int.Parse(textBoxHour.Text) == hour && int.Parse(textBoxMin.Text) == min && sec <= 1)
                {
                    button1_Click(null, null);
                }
            }
        }


        private void button2_Click(object sender, EventArgs e)
        {
            
            if(checkAdmin())
            {
                MessageBox.Show("设置开机自启动，需要修改注册表，请确认本程序已解压到本地文件夹", "提示");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.SetValue("autoLogin", path);
                rk2.Close();
                rk.Close();
            }
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (checkAdmin())
            {
                MessageBox.Show("取消开机自启动，需要修改注册表", "提示");
                string path = Application.ExecutablePath;
                RegistryKey rk = Registry.LocalMachine;
                RegistryKey rk2 = rk.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run");
                rk2.DeleteValue("autoLogin", false);
                rk2.Close();
                rk.Close();
            }
        }

        void loadData()//---------------load
        {
            if (File.Exists(authData))
            {
                checkBox1.Checked = true;

                try
                {
                    StreamReader sr = new StreamReader(authData);
                    authDataStr = sr.ReadLine();
                    sr.Close();
                    
                    authDataStrAll = authDataStr.Split(new string[] { "##" }, StringSplitOptions.None);
                    //MessageBox.Show("已加载账号 "+authDataStrAll[1]+" 的数据", "", MessageBoxButtons.OK);
                    if (authDataStrAll[0] == "1")
                    {
                        idNum.Text = authDataStrAll[1];
                        password.Text = authDataStrAll[2];
                        textURL.Text = authDataStrAll[3];
                        radioButton1.Checked = true;
                        mode = 1;
                    }
                    else if (authDataStrAll[0] == "2")
                    {
                        idNum.Text = authDataStrAll[1];
                        password.Text = authDataStrAll[2];
                        textURL.Text = authDataStrAll[3];
                        radioButton2.Checked = true;
                        mode = 2;
                    }
                    


                    if (authDataStrAll[5] != "")
                    {
                        textBoxHour.Text = authDataStrAll[4];
                        textBoxMin.Text = authDataStrAll[5];
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message.ToString(), "", MessageBoxButtons.OK);
                }
                finally
                {
                    button1_Click(null, null);
                }
            }
        }

        void saveData()//--------------save
        {
            StreamWriter sw = new StreamWriter(authData);
            string temp = mode + "##" + idNum.Text + "##" + password.Text + "##" + textURL.Text + "##" + textBoxHour.Text + "##" + textBoxMin.Text;
            sw.Write(temp);
            sw.Flush();
            sw.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            webBrowser1.Navigate(cyber);
            textURL.Text = cyber;
        }

        

        bool checkAdmin()
        {
            System.Security.Principal.WindowsIdentity identity = System.Security.Principal.WindowsIdentity.GetCurrent();
            System.Security.Principal.WindowsPrincipal principal = new System.Security.Principal.WindowsPrincipal(identity);
            //判断当前登录用户是否为管理员
            if (principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator))
            {
                //如果是管理员，则直接运行
                return true;
            }
            else
            {
                //创建启动对象
                System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                //设置启动动作,确保以管理员身份运行
                startInfo.Verb = "runas";
                try
                {
                    
                    //退出
                    Location = new Point(0, 0);
                    MessageBox.Show("正在用管理员权限重新启动，请再次点击此功能", "切换管理员权限提示",MessageBoxButtons.OK);
                    this.Close();
                    System.Diagnostics.Process.Start(startInfo);
                }
                catch
                {
                }
                return false;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:iamlocky@qq.com");
        }
    }
}

