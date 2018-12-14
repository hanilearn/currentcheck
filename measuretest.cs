using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CurrentCheckTool
{
    public class measuretest
    {
        Form1 form;
        public Multimeter[] multimeters;
        public Multimeter[] Multimeters
        {
            set { multimeters = value; }
        }

        public measuretest(Form1 frm)
        {
            this.form = frm;
        }

        public void reset()
        {
            foreach (var multimeter in multimeters)
            {
                if (!multimeter.Terminated)
                {
                    multimeter.ComClose();
                    multimeter.tmrStop();
                }
                multimeter.Terminated = false;
                multimeter.Passed = false;

            }
        }

        public void pause()
        {
            foreach (var multimeter in multimeters)
            {
                if (!multimeter.Terminated)
                {
                    //multimeter.ComClose();
                    //multimeter.tmrStop();
                    multimeter.TimeOut = true;
                }
                //multimeter.Terminated = false;                

            }
        }

        public string[] LogData = new string[3];
        public bool gotest()
        {
            int x = 0;
            bool failtest = false;
            foreach (var Multimeter in multimeters)
            {
                x++;
                if (!Multimeter.Passed)
                {
                    Multimeter.avswitch = form._AVSwitch;
                    this.form.showMSG(x, "Testing");
                    Multimeter.Meterindex = x;
                    Multimeter.Domeasure();
                    while (true)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        if (Multimeter.Terminated == true)
                            break;
                    }
                }
                this.form.showMSG(x, Multimeter._OutputMessage);
                if (Multimeter._OutputMessage == "Test Fail")
                {
                    failtest = true;
                    break;
                }
                else
                {
                    double b = 0;
                    string tmp = Multimeter.Log.TrimEnd(',').ToString();
                    foreach (var item in tmp.Split(','))
                    {
                        b += double.Parse(item);
                    }
                    this.form.textBox8.Text = this.form.textBox8.Text + Multimeter.Monitor[0] + Environment.NewLine;
                    this.form.textBox8.Text = this.form.textBox8.Text + tmp + Environment.NewLine;

                    LogData[x - 1] = decimal.Parse((b / 3).ToString(), System.Globalization.NumberStyles.Float).ToString();
                }
            }
            if (failtest)
            {
                form.panelshow(true, "NG");
            }

            return failtest;
        }

        public int Gotest_byHotKey()
        {
            int x = 0;
            bool failtest = false;
            foreach (var Multimeter in multimeters)
            {
                x++;
                if (Multimeter.Passed)
                    continue;
                else if (!Multimeter.Passed)
                {
                    Multimeter.avswitch = form._AVSwitch;
                    this.form.showMSG(x, "Testing");
                    Multimeter.Meterindex = x;
                    Multimeter.Domeasure();
                    while (true)
                    {
                        System.Windows.Forms.Application.DoEvents();
                        if (Multimeter.Terminated == true)
                            break;
                    }
                }
                this.form.showMSG(x, Multimeter._OutputMessage);
                if (Multimeter._OutputMessage == "Test Fail")
                {
                    failtest = true;
                    break;
                }
                else
                {
                    double b = 0;
                    string tmp = Multimeter.Log.TrimEnd(',').ToString();
                    foreach (var item in tmp.Split(','))
                    {
                        b += double.Parse(item);
                    }
                    this.form.textBox8.Text = this.form.textBox8.Text + Multimeter.Monitor[0] + Environment.NewLine;
                    this.form.textBox8.Text = this.form.textBox8.Text + tmp + Environment.NewLine;

                    LogData[x - 1] = decimal.Parse((b / 3).ToString(), System.Globalization.NumberStyles.Float).ToString();

                    if (x == 3)
                        return 0;   //測試結束
                    break;
                }


            }
            if (failtest)
            {
                form.panelshow(true, "NG");
            }

            return 1;   //測試未完成
        }

    }

}
