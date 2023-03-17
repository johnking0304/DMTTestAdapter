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
using TAI.Manager;
using TAI.Modules;
using TAI.Device;

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

            this.comboBoxAnaChannel.SelectedIndex = 0;
            this.comboBoxStart.SelectedIndex = 0;
            this.comboBoxtarget.SelectedIndex = 0;
            this.comboBoxMode.SelectedIndex = 0;
            this.comboBoxGtype.SelectedIndex = 0;
            this.comboBoxMType.SelectedIndex = 0;
            this.comboBoxSwitchMode.SelectedIndex = 0;

        }


        private void button1_Click_1(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.Initialize();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.AppendText(this.TestAdapter.Initialize()); 
        }

        private void AppendText(string content)
        {
            this.richTextBox1.AppendText(string.Format("{0}{1}", content, "\r"));
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.TestAdapter.SwitchController.SwitchModeOperate((SwitchMode)this.comboBoxSwitchMode.SelectedIndex);
            this.TestAdapter.SwitchController.SwitchChannelOperate((ushort)(this.comboBoxAnaChannel.SelectedIndex+1)); ;
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void button4_Click(object sender, EventArgs e)
        {
            string content = this.comboBoxStart.Text;
            string[] list = content.Split('-');
            int start = int.Parse(list[0]);

            content = this.comboBoxtarget.Text;
            list = content.Split('-');
            int targent = int.Parse(list[0]);

            ActionMode mode = (ActionMode)(this.comboBoxMode.SelectedIndex + 1);

            this.TestAdapter.ProcessController.SetRobotMoveParams(start, targent, mode);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.SetRobotMoveEnable();
        }

        private void comboBoxStart_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string content = "";
            bool result = this.TestAdapter.VISController.QRModelSerialCode(ref content);
            this.textBoxModuleSerialCode.Text = string.Format("{0}:{1}", result ? "OK" : "Error", content);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string content = "";
            bool result = this.TestAdapter.VISController.OCRModelType(ref content);
            this.textBoxModuleType.Text = string.Format("{0}:{1}",result?"OK":"Error",content);

        }

        private void button14_Click(object sender, EventArgs e)
        {
            string content = "";
            int channel = comboBoxModuleChannel.SelectedIndex + 1;
            ModuleType moduleType = (ModuleType)(comboBoxModuleType.SelectedIndex + 1);
            bool result = this.TestAdapter.VISController.OCRChannelLighting(channel, moduleType, ref content);
            this.richTextBoxLighting.Text = string.Format("{0}:{1}", result ? "OK" : "Error", content);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.InitializeSystem();
        }


        private void AddItem(string name, string value)
        {
            ListViewItem item = new ListViewItem();
            item.Text = name;
            item.SubItems.Add(value);
            this.listView1.Items.Add(item);
        }

        private void AddItem(string name, bool value)
        {
            ListViewItem item = new ListViewItem();
            item.Text = name;
            item.SubItems.Add(value? "True" : "False" );
            this.listView1.Items.Add(item);
        }
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (this.TestAdapter.ProcessController.Active())
            {


                this.listView1.BeginUpdate();
                this.listView1.Items.Clear();
                this.AddItem("流控PLC初始化完成", this.TestAdapter.ProcessController.InitializeCompleted);

                this.AddItem("机械手到达位置", this.TestAdapter.ProcessController.RobotMoveCompleted);
                this.AddItem("机械手空闲状态", this.TestAdapter.ProcessController.RobotIdle);








                this.listView1.EndUpdate();
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (this.TestAdapter.GeneratorDevice.Initialize())
            {
                this.groupBoxGen.Text = string.Format("模拟量信号发生器-{0}", this.TestAdapter.GeneratorDevice.Identify);
                this.buttonGSet.Visible = true;
            }
            else
            {
                string.Format("模拟量信号发生器-Error");
            }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            if (this.TestAdapter.MeasureDevice.Initialize())
            {
                this.groupBoxGen.Text = string.Format("模拟量信号测量器-{0}", this.TestAdapter.MeasureDevice.Identify);
                this.buttonMGet.Visible = true;
            }
            else
            {
                this.groupBoxGen.Text = string.Format("模拟量信号测量器-Error");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            float value = 0;
            ChannelType type = (ChannelType)comboBoxGtype.SelectedIndex;
            if (float.TryParse(this.textBoxGValue.Text, out value))
            {              
                this.TestAdapter.GeneratorDevice.SetValue(type, value);
            }
            
        }

        private void comboBoxGtype_SelectedIndexChanged(object sender, EventArgs e)
        {
           // this.textBoxGValue.Clear();
            this.textBoxGValue.Text = "0";
        }

        private void button9_Click(object sender, EventArgs e)
        {
            float value = 0;
            ChannelType type = (ChannelType)comboBoxMType.SelectedIndex;
            if (this.TestAdapter.MeasureDevice.GetValue(type, ref value))
            {
                this.textBoxMValue.Text = value.ToString();
            }
            else
            {
                this.textBoxMValue.Text = "Error";
            }


        }
    }
}
