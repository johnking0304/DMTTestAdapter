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
    public partial class FormAddUser : Form
    {
        public User user = new User();
        public FormAddUser()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            this.user.Name = this.textBoxUserName.Text;
            this.user.Password = this.PasswordTextBox.Text;
            if (CheckUser(user.Name)) {
                Windows.MessageBoxError("已存在同名用户，请重新输入！");
                return;
            }
            MysqlSugarHelper.Insert(user);
            Windows.MessageBoxInformation("插入成功！");
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool CheckUser(string userName)
        {
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Single();
            return user != null;
        }
    }
}
