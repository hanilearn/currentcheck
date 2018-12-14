using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CurrentCheckTool
{

    public partial class Form1 : Form 
    {
        shopfloor sfhandle;
        measuretest measure;

        public bool _AVSwitch;
        public Form1()
        {
            InitializeComponent();
            sfhandle = new shopfloor(this);
            //measure = new measuretest(this);
            
        }
        
        public delegate void showtext(string data);
        public void showstr(string data)
        {
            textBox1.AppendText(data);
            //data = data.Replace("\r", "");
            //Console.WriteLine(data);
        }

        public void showMSG( int x , string data)
        {
            if(x==1)
                textBox5.Text = data;
            else if(x==2)
                textBox6.Text = data;
            else if (x==3)
                textBox7.Text = data;
        }


        List<Multimeter> readfiletostringlist()
        {
            List<Multimeter> MultimeterList = new List<Multimeter>();

            string[] txtSCPI;
            string comport = "";
            string cmpdatas = "";
            string closecomport = "";
            List<string> list_monitor = new List<string>();
            List<string> list_command = new List<string>();
            List<string> lst_com = new List<string>();

            MultimeterList.Clear();
            int x = 0;

            txtSCPI = File.ReadAllLines("SCPI.txt");
            foreach (string fs in txtSCPI)
            {
                if (fs.Length == 0)
                    continue;
                int splitidx = fs.IndexOf(":");
                if (fs == "End")
                    break;
                else if (fs.Substring(0, splitidx) == "Open")
                {
                    x++;
                    comport = fs.Substring(splitidx + 1, fs.Length - 1 - (fs.Substring(0, splitidx).Length));
                    lst_com.Add(comport);
                }
                else if (fs.Substring(0, splitidx) == "Monitor")
                {
                    //qrcodeSN.AppendText(fs.Substring(splitidx + 1, fs.Length - 1 - (fs.Substring(0, splitidx).Length)));
                    list_monitor.Add(fs.Substring(splitidx + 1, fs.Length - 1 - (fs.Substring(0, splitidx).Length)));
                }
                else if (fs.Substring(0, splitidx) == "SCPI" || fs.Substring(0, splitidx) == "Delay" || fs.Substring(0, splitidx) == "Recv")
                {
                    list_command.Add(fs);
                }
                else if (fs.Substring(0, splitidx) == "Compare")
                {
                    cmpdatas = fs.Substring(splitidx + 1, fs.Length - 1 - (fs.Substring(0, splitidx).Length));
                }
                else if (fs.Substring(0, splitidx) == "Close")
                {
                    closecomport = fs;
                    
                    MultimeterList.Add(new Multimeter(comport, list_monitor.ToArray(), list_command.ToArray(), cmpdatas, closecomport , textBox1 ));
                    comport = closecomport = cmpdatas = "";
                    list_monitor.Clear();
                    list_command.Clear();

                }
                
            }
            if (lst_com[x-2] == lst_com[x-1])
                _AVSwitch = true;

            return MultimeterList;
        }

        private void button5_Click(object sender, EventArgs e)
        {

            sfhandle.Multimeters = readfiletostringlist().ToArray();


            //measure.Multimeters = readfiletostringlist().ToArray();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //1.reset 所有狀態
            sfhandle.sf_reset();
            //2.掃序號
            qrcodeSN.Text = "";
            qrcodeSN.Focus();
            //3.            
            textBox5.Text = "uncheck";            
            textBox6.Text = "uncheck";            
            textBox7.Text = "uncheck";

        }

        private void qrcodeSN_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                textBox5.Text = "uncheck";
                textBox6.Text = "uncheck";
                textBox7.Text = "uncheck";
                //button2.Visible = true;
                //button3.Visible = true;
                //button4.Visible = true;

                sfhandle.sf_reset();
                string strCurrentString = qrcodeSN.Text.Trim().ToString();
                if (strCurrentString != "")
                {
                    sfhandle.Mo_number = strCurrentString;
                    sfhandle.sfSN = sfhandle.Mo_number;
                    rtnbysfc.Text = sfhandle.shopfloorcheck();
                    if (sfhandle.SfcError)
                    {
                        panel1.Visible = true;
                        NGLabel.Visible = true;
                        OKLabel.Visible = false;
                    }
                    else
                    {
                        panel1.Visible = false;
                        sfhandle.sf_reset();
                        //sfhandle.sf_gotest(); 改使用外部快捷鍵執行(預設 F5)                                                
                    }
                }               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox5.Text == "Testing")
            {
                panel1.Visible = false;
                sfhandle.sf_pause();
                //measure.pause();
            }
            else if (textBox5.Text == "Test Fail")
            {
                panel1.Visible = false;
                sfhandle.sf_gotest();
                //measure.gotest();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (textBox6.Text == "Testing")
            {
                panel1.Visible = false;
                sfhandle.sf_pause();
                //measure.pause();
            }
            else if (textBox6.Text == "Test Fail")
            {
                panel1.Visible = false;
                sfhandle.sf_gotest();
                //measure.gotest();
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (textBox7.Text == "Testung")
            {
                panel1.Visible = false;
                sfhandle.sf_pause();
                //measure.pause();
            }
            else if (textBox7.Text == "Test Fail")
            {
                panel1.Visible = false;
                sfhandle.sf_gotest();
                //measure.gotest();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox5.Text = "uncheck";
            textBox6.Text = "uncheck";
            textBox7.Text = "uncheck";
            panel1.Visible = false;
            measure.reset();
            measure.gotest();
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            button2.Enabled = true;
            if (textBox5.Text == "uncheck" || textBox5.Text == "Testing")
                button2.Text = "Pause";
            else if (textBox5.Text == "Test Fail")
                button2.Text = "Again";
            else if (textBox5.Text == "PASS")
                button2.Enabled = false;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            button3.Enabled = true;
            if (textBox6.Text == "uncheck" || textBox6.Text == "Testing")
                button3.Text = "Pause";
            else if (textBox6.Text == "Test Fail")
                button3.Text = "Again";
            else if (textBox6.Text == "PASS")
                button3.Enabled = false;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            button4.Enabled = true;
            if (textBox7.Text == "uncheck" || textBox7.Text == "Testing")
                button4.Text = "Pause";
            else if (textBox7.Text == "Test Fail")
                button4.Text = "Again";
            else if (textBox7.Text == "PASS")
                button4.Enabled = false;
        }

        public void panelshow(bool show, string result )
        {
            if (show)
            {
                panel1.Visible = true;
                if (result == "OK")
                {
                    OKLabel.Visible = true;
                    NGLabel.Visible = false;
                }
                else if (result == "NG")
                {
                    OKLabel.Visible = false;
                    NGLabel.Visible = true;
                }
                qrcodeSN.Focus();
                qrcodeSN.SelectAll();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            button5.PerformClick();
        }
        
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:
                    //if(e.Control)
                        sfhandle.sf_gotest();
                    break;
            }                            
        }
    }



}
