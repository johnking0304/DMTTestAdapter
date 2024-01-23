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
using TAI.Test.Scheme;

namespace DMTTestStation
{
    public partial class FormDeleteUser : Form
    {
        public FormDeleteUser()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (this.textBoxUserName.Text == "") {
                Windows.MessageBoxError("请输入用户名！");
                return;
            }
            if (!CheckUser(this.textBoxUserName.Text)) {
                Windows.MessageBoxError("用户名不存在！");
                return;
            }
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == this.textBoxUserName.Text).Single();
            MysqlSugarContext.MysqlSugarDB.Deleteable(user);
        }

        private bool CheckUser(string userName)
        {
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Single();
            return user != null;
        }

        private void ResetPasswordButton_Click(object sender, EventArgs e)
        {
            if (this.textBoxUserName.Text == "")
            {
                Windows.MessageBoxError("请输入用户名！");
                return;
            }
            if (!CheckUser(this.textBoxUserName.Text))
            {
                Windows.MessageBoxError("用户名不存在！");
                return;
            }
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == this.textBoxUserName.Text).Single();
            user.Password = TextString.MD5Encrypt32(this.textBoxUserName.Text);
            MysqlSugarContext.MysqlSugarDB.Updateable(user);


        }
    }
}
