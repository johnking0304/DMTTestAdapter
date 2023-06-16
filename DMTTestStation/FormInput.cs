using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DMTTestStation
{
    public partial class FormInput : Form
    {
        public string SerialCode { get; set; }
        public FormInput(string caption)
        {
            InitializeComponent();
            this.groupBoxInput.Text = caption;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.SaveAndClose();
        }

        private void SaveAndClose()
        {
            this.SerialCode = this.textBoxInput.Text.Trim();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void FormInput_Load(object sender, EventArgs e)
        {
            
            this.SerialCode = "";
            this.textBoxInput.Clear();
            this.textBoxInput.Focus();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                this.SaveAndClose();
            }
        }
    }
}
