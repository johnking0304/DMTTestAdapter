using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMT.DatabaseAdapter;
using TAI.Test.Scheme;
using DMT.Core.Utils;

namespace DMTTestStation
{
    public partial class FormLogin : Form
    {
        
        public FormLogin()
        {
            InitializeComponent();
        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string userName = this.textBoxUserName.Text;
            string password = this.textBoxPassword.Text;
            password = TextString.MD5Encrypt32(password);

            if (this.Login(userName, password))
            {
                DialogResult = DialogResult.OK;
                Program.StationManager.UserName = userName;
                this.Close();
            }
            else
            {
                Windows.MessageBoxError("用户名或密码错误，登录失败！");
            }
        }

        private bool Login(string userName, string password)
        {
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Where(it => it.Password == password).Single();
            return  user != null ;            
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
