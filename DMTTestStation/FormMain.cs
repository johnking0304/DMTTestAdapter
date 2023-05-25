using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.Test.Scheme;
using DMT.DatabaseAdapter;

namespace DMTTestStation
{
    public partial class FormMain : Form
    {

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {


        }

        private void button1_Click(object sender, EventArgs e)
        {
            MysqlSugarContext.ConnectMysqlTest("127.0.0.1", "3306", "testschema", "root", "123456");

            MysqlSugarContext.SetupMysql("127.0.0.1", "3306", "testschema", "root", "123456");
        }
    }
}
