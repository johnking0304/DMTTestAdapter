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
using TAI.StationManager;
using DMTTestStation;
using System.Diagnostics;

namespace DMTTestStation
{
    public partial class FormLogin : Form
    {

        string reviewerName { get; set; }
        public FormLogin()
        {
            InitializeComponent();
        }


        private void buttonLogin_Click(object sender, EventArgs e)
        {
            this.doLogin();
        }

        private void doLogin() {
            string userName = this.textBoxUserName.Text;
            string password = this.textBoxPassword.Text;
            password = TextString.MD5Encrypt32(password);
            // 先判断测试执行人员和复核人员不能为同一人员
            if (userName == Program.StationManager.reviewer) 
            {
                Windows.MessageBoxError("复核人员不可与测试执行人员相同，请更改用户！");
                return;
            }
            Program.StationManager.SiteName = comboBoxSite.Text;

            if (this.Login(userName, password))
            {
                DialogResult = DialogResult.OK;
                Program.StationManager.UserName = userName;
                if (!Program.FormMainIsExist)
                {
                    Program.FormMainIsExist = true;
                    this.Hide();
                    Form formMain = new FormMain();
                    formMain.ShowDialog();
                }
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

        private void FormLogin_Load(object sender, EventArgs e)
        {
            Program.FormLoginIsExist = true;
            if (Program.FormMainIsExist) {
                this.textBoxUserName.Text = Program.StationManager.UserName;
                this.button2.Visible = true;
                if (this.textBoxUserName.Text == "Snpas") {
                this.addUserButton.Visible = true;
                this.DeleteUserButton.Visible = true;
                }
            }
            Program.StationManager = new StationManager();
            Program.StationManager.Initialize();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Form formUser =  new FormUser();
            formUser.ShowDialog();
        }

        private void login(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter) { 
                this.doLogin();
            }
        }

        private void addUserButton_Click(object sender, EventArgs e)
        {
            FormAddUser addUser =  new FormAddUser();
            addUser.ShowDialog();
                        
        }

        private void DeleteUserButton_Click(object sender, EventArgs e)
        {
            FormDeleteUser deleteUser = new FormDeleteUser();
            deleteUser.ShowDialog();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if(checkBox1.Checked)
            {
                FormReviewer formReviewer = new FormReviewer();
                formReviewer.callbackReviewer = handle_reviewerForm;
                formReviewer.ShowDialog();
            }
        }

        private void handle_reviewerForm(string reviewerName) {
            if (reviewerName.Length == 0) {
                checkBox1.Checked = false;
            }
            else {
                Program.StationManager.reviewer = reviewerName;
                checkBox1.Checked = true;
            }

        }
    }
}
