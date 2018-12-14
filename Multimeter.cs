using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace CurrentCheckTool
{
    public class Multimeter
    {
        string com;
        string[] monitor;
        string[] commandset;
        string cmpdata;
        string closecom;
        bool bPassed;

        bool terminated;
        System.Timers.Timer locRecvTimer;
        string OutputMessage;

        private SerialPort port;

        private string cmpMax, cmpMin;

        private int dataChkCnt;
        public static TextBox _textbox;

        public string Log;

        int meterindex;
        public int Meterindex
        {
            set { meterindex = value; }
        }
        public bool avswitch;

        public Multimeter(string com, string[] monitor, string[] commandset, string cmpdata, string closecom, TextBox tb1)
        {
            _textbox = tb1;
            if (com == null)
                this.com = "NONE";
            else
                this.com = com;
            this.monitor = monitor;
            this.commandset = commandset;
            this.cmpdata = cmpdata;
            this.closecom = closecom;


            port = new SerialPort(this.com, 9600, Parity.None, 8, StopBits.One);
            port.DtrEnable = true;

            port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);

            locRecvTimer = new System.Timers.Timer();
            locRecvTimer.Elapsed += new System.Timers.ElapsedEventHandler(_TimersTimer_Elapsed);


        }

        delegate void PrintHandler(TextBox tb, string text);
        public static void Print(TextBox tb, string text)
        {
            //判斷這個TextBox的物件是否在同一個執行緒上
            if (tb.InvokeRequired)
            {
                //當InvokeRequired為true時，表示在不同的執行緒上，所以進行委派的動作!!
                PrintHandler ph = new PrintHandler(Print);
                tb.Invoke(ph, tb, text);
            }
            else
            {
                //表示在同一個執行緒上了，所以可以正常的呼叫到這個TextBox物件
                //tb.Text = tb.Text + text + Environment.NewLine;
                tb.Text = text;
            }
        }


        public bool Terminated
        {
            get { return terminated; }
            set { terminated = value; }
        }

        public bool Passed
        {
            get { return bPassed; }
            set { bPassed = value; }
        }

        public string Com
        {
            get { return com; }
            set { com = value; }
        }

        public string[] Monitor
        {
            get { return monitor; }
            set { monitor = value; }
        }

        public string[] Commandset
        {
            get { return commandset; }
            set { commandset = value; }
        }

        public string Cmpdata
        {
            get { return cmpdata; }
            set { cmpdata = value; }
        }

        public string Closecom
        {
            get { return closecom; }
            set { closecom = value; }
        }

        public string _OutputMessage
        {
            get { return OutputMessage; }
        }

        public void tmrStop()
        {
            locRecvTimer.Stop();
        }

        public void ComClose()
        {
            port.Dispose();
            port.Close();

            terminated = true;
        }

        bool bTimeOut;
        bool bComGetted;

        public bool TimeOut
        {
            set { bTimeOut = value; }
        }

        bool fluke45;

        void chkmltmt(string Data)
        {
            string tmp = "meas?";
            //Data.sp
            if (Data.ToLower().IndexOf(tmp) >= 0)
                fluke45 = true;
        }

        public void Domeasure()
        {
            terminated = false;
            bTimeOut = false;
            bComGetted = false;
            dataChkCnt = 0;
            Log = "";

            if (this.cmpdata.IndexOf("/") > 0)
            {
                string[] tmp = this.cmpdata.Split('/');
                cmpMin = tmp[0];
                cmpMax = tmp[1];
                if (float.Parse(cmpMax) < float.Parse(cmpMin))
                {
                    cmpMax = tmp[0];
                    cmpMin = tmp[1];
                }

            }
            else
            {
                cmpMax = cmpMin = this.cmpdata;

            }

            if(!port.IsOpen)
                port.Open();
            port.DiscardInBuffer();
            port.DiscardOutBuffer();

            foreach (string cmd in this.commandset)
            {
                int splitidx = cmd.IndexOf(":");
                string data;
                if (cmd.Length - 1 == splitidx)
                    data = "0";
                else
                    data = (cmd.Substring(splitidx + 1, cmd.Length - 1 - (cmd.Substring(0, splitidx).Length)));

                if (cmd.Substring(0, splitidx) == "SCPI")
                {
                    chkmltmt(data);
                    port.Write(String.Format("{0}{1}", data, "\n"));

                }
                else if (cmd.Substring(0, splitidx) == "Delay")
                {
                    Thread.Sleep(Int32.Parse(data));
                }
                else if (cmd.Substring(0, splitidx) == "Recv")
                {
                    int Interval;

                    if (data != "" && Int32.TryParse(data, out Interval))
                        locRecvTimer.Interval = Int32.Parse(data);

                    locRecvTimer.Start();
                }
            }
        }

        void _TimersTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //在這裡做想做的事 label1.Text = (int.Parse(label1.Text) + 1).ToString();

            locRecvTimer.Enabled = false;
            if (bComGetted == false)
            {
                bTimeOut = true;
                return;
            }
            if (bPassed != true)
            {

                OutputMessage = "Test Fail";
                ComClose();

            }
        }

        private void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string tmp = port.ReadLine();
            Print(_textbox, tmp);
            bComGetted = true;
            tmp = tmp.Replace("\r", "");
            tmp = tmp.Replace("\n", "");
            tmp = tmp.Replace("=", "");
            tmp = tmp.Replace(">", "");
            tmp = tmp.Replace("?", "");

            data_compare(tmp);
            if (bTimeOut)
            {
                if (bPassed != true)
                {
                    OutputMessage = "Test Fail";
                    ComClose();
                }
            }

            if (bPassed != true && bTimeOut == false)
            {
                bComGetted = false;
                port.DiscardOutBuffer();
                port.DiscardInBuffer();
                if (fluke45)
                {
                    Thread.Sleep(500);
                    port.Write("meas?\n");
                    Thread.Sleep(800);

                }
                else
                    port.Write("READ?\n");
            }

            if (bPassed)
            {
                //port.ReadLine();
            }
        }

        private void data_compare(string data)
        {
            double n;
            if (data != null)
            {
                bool isNumeric = double.TryParse(data, out n);
                if (isNumeric && n <= double.Parse(cmpMax) && n >= double.Parse(cmpMin))
                {
                    //                    Log = Log + n.ToString() + ",";

                    string tmp = decimal.Parse(n.ToString(), System.Globalization.NumberStyles.Float).ToString();
                    Log = Log + tmp + " ,";

                    dataChkCnt++;
                    if (dataChkCnt == 3)
                    {
                        bPassed = true;

                        locRecvTimer.Enabled = false;
                        this.OutputMessage = "PASS";

                        if (meterindex == 2 && avswitch)
                        {
                            if (fluke45)
                            {
                                Thread.Sleep(500);
                                port.Write("vdc\n");
                            
                            }
                            else
                            {
                                port.Write("MEAS:VOLT:DC?\n");
                            }
                            


                        }
                        else if (meterindex == 3 && avswitch)
                        {
                            if (fluke45)
                            {
                                Thread.Sleep(500);
                                port.Write("adc\n");
                            
                            }
                            else
                            {
                                port.Write("MEAS:CURR:DC?\n");
                            }
                            
                        }
                        else
                        {
                            //ComClose();
                        }

                        ComClose();
                    }
                }
            }
        }

    }

}
