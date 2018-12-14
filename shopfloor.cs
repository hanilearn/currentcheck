using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CurrentCheckTool
{
    public class shopfloor : measuretest
    {
        Form1 form;
        string basicinfo="";
        string MO_number;
        string tdata;
        public string sfSN;

        bool sfcerror;

        [DllImport("SFC_DLL.dll", EntryPoint = "SET_BASIC_INFO")]
        static extern string SET_BASIC_INFO( string basicinfo);
        [DllImport("SFC_DLL.dll", EntryPoint = "DEAL_TEST_DATA")]
        static extern string DEAL_TEST_DATA( string routingstr, string sfdata, string sflog);
        [DllImport("SFC_DLL.dll", EntryPoint = "DEAL_TEST_DATA_EX")]
        static extern int DEAL_TEST_DATA_EX(string routingstr, string sfdata, ref string sflog);

        public void sf_reset()
        {
            this.MO_number = "";
            this.tdata = "";
            this.sfcerror = false;
            reset();
        }

        public void sf_pause()
        {
            pause();
        }

        public shopfloor(Form1 frm) : base(frm)
        {
            this.form = frm;
            this.basicinfo = this.shopfloorinfo();
            this.tdata = "";
        }

        public bool SfcError
        {
            get { return sfcerror; }
        }

        public string Mo_number
        {
            get { return MO_number; }
            set { MO_number = value; }
        }

        
        string shopfloorinfo()
        {
            return "YES";

            string output = "";
            try
            {
                output = SET_BASIC_INFO(DBNAME, basicinfo);
            }
            catch (DllNotFoundException)
            {
                try
                {
                    DEAL_TEST_DATA_EX( routingstr,  sfdata, ref  sflog);
                }
                catch (DllNotFoundException)
                {

                }

            }
            return output;
        }

        void checkrtn(int rtn, string str)
        {
            if (rtn == -1)
            {
                if (str.Substring(0, 1) == "1")
                {
                    sfcerror = true;
                }

            }
            else
            {
                if (rtn == 1)
                    sfcerror = true;
            }

        }

        public string shopfloorcheck()
        {
            return "YES";

            string output = "";
            int rtn = -1;
            try
            {
                output = DEAL_TEST_DATA(  routingstr,  sfdata,  sflog);


            }
            catch (DllNotFoundException)
            {
                try
                {
                    rtn = DEAL_TEST_DATA_EX( routingstr,  sfdata, ref  sflog);
                }
                catch (DllNotFoundException)
                {

                }

            }
            checkrtn(rtn, output);

            return output;
        }

        public void sf_gotest()
        {
            if (Gotest_byHotKey() == 0)    //測試全部PASS
            //if(!gotest())            
            {
                addlogdata();
                this.form.rtnbysfc.Text = shopfloorupdate();
                if (sfcerror)
                    form.panelshow(true, "NG");
                else
                    form.panelshow(true, "OK");
            }

        }

        public void addlogdata()
        {
            this.tdata = string.Join(" ,", LogData);
        }

        string makelogstr()
        {
            return
                string.Format("[\r\n[\"{0}\",\"{1}\",\"\",\"\",\"\",\"\",\"\",0,\"\",\"\" ]\r\n]", this.tdata, DateTime.Now.ToShortTimeString());
        }

        public string shopfloorupdate()
        {
            return "YES";

            string log_data = this.makelogstr();

            string output = "";
            int rtn = -1;
            try
            {
                
                output = DEAL_TEST_DATA( routingstr,  sfdata,  sflog);

            }
            catch (DllNotFoundException)
            {
                try
                {
                    rtn = DEAL_TEST_DATA_EX( routingstr,  sfdata, ref  sflog);
                    output = log_data;
                }
                catch (DllNotFoundException)
                {

                }
            }

            checkrtn(rtn, output);

            return output;
        }

    }

}