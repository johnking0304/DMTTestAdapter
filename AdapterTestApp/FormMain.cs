using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMTTestAdapter;
using TAI.Manager;

namespace AdapterTestApp
{
    public partial class FormMain : Form
    {
        public TestAdapter TestAdapter { get; set; }

        public FormMain()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {


        }

        private void FormMain_Load(object sender, EventArgs e)
        {


  
        }


        public void NotifyMessage(int Event, int code, string message)
        {
            button1.Text = string.Format("{0}-{1}-{2}", Event, code, message);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            object temp1 = 0.2;
            object temp2 = 0.4;
            if (((double)temp1 - (double)temp2) > 0.1)
            { 
                
            }
        }
    }
}
