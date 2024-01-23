using DMT.Core.Utils;
using DMT.DatabaseAdapter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using TAI.Test.Scheme;

namespace DMTTestStation
{
    public partial class FormReviewer : Form
    {
        public delegate void CallbackReviewer(string reviewerName);

        public CallbackReviewer callbackReviewer = null;
        public bool isLogin = false;
        public FormReviewer()
        {
            InitializeComponent();
        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void buttonLogin_Click(object sender, EventArgs e)
        {
            string userName = this.textBoxUserName.Text;
            string password = this.textBoxPassword.Text;
            password = TextString.MD5Encrypt32(password);

            if (this.Login(userName, password))
            {
                this.Invoke(callbackReviewer, userName);
                isLogin = true;
                this.Close();
            }
            else
            {
                Windows.MessageBoxError("用户名或密码错误，登录失败！");
                return;
            }
            
        }
        private bool Login(string userName, string password)
        {
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Where(it => it.Password == password).Single();
            return user != null;
        }
        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Invoke(callbackReviewer, string.Empty);
            this.Close();
        }

        private void FormReviewer_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(!isLogin)
            {
                this.Invoke(callbackReviewer, string.Empty);
            }
        }
    }
}
