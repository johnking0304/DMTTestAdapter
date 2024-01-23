using DMT.Core.Utils;
using DMT.DatabaseAdapter;
using System;
using System.Windows.Forms;
using TAI.Test.Scheme;

namespace DMTTestStation
{
    public partial class FormUser : Form
    {
        public FormUser()
        {
            InitializeComponent();
        }

        private void ConfirmButton_Click(object sender, EventArgs e)
        {
            string oldPassWord = this.OldPasswordTextBox.Text;
            string newPassWord = this.NewPasswordtextBox.Text;
            string userName = this.textBoxUserName.Text;
            if (!CheckUser(userName, oldPassWord))
            {
                Windows.MessageBoxError("密码错误！");
                return;
            }
            if (CheckPassWordSame(oldPassWord, newPassWord))
            {
                Windows.MessageBoxError("新旧密码相同！");
                return;
            }
            if (!this.UpdateUser(userName, TextString.MD5Encrypt32(newPassWord)))
            {
                Windows.MessageBoxInformation("密码更新失败！");
                return;
            }
            Windows.MessageBoxInformation("密码更新成功！");

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private bool CheckUser(string userName, string password)
        {
            String securedPassword = TextString.MD5Encrypt32(password);
            var user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Where(it => it.Password == securedPassword).Single();
            return user != null;
        }
        private bool CheckPassWordSame(string oldPassWord, string newPassWord)
        {
            return oldPassWord == newPassWord;
        }

        private bool UpdateUser(string userName, string newPassword)
        {
            User user = MysqlSugarContext.MysqlSugarDB.Queryable<User>().Where(it => it.Name == userName).Single();
            user.Name = userName;
            user.Password = newPassword;
            return MysqlSugarHelper.Update(user);
        }

    }
}
