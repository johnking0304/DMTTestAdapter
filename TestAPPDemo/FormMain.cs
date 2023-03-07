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
using DMT.Core.Utils;

namespace TestAPPDemo
{
    public partial class FormMain : Form
    {
        public TestAdapter TestAdapter { get; set; }
        public FormMain()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.TestAdapter = new TestAdapter();
            this.TestAdapter.Initialize();
                }

        private void button1_Click(object sender, EventArgs e)
        {

            LogHelper.LogInfoMsg("123");
        }
    }
}
