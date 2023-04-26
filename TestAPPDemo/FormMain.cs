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
            this.comboBoxStation.SelectedIndex = 0;
            this.comboBoxChannelCount.SelectedIndex = 0;
            this.comboBoxDeviceType.SelectedIndex = 0;


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
            /*            string[] lines = this.comboBoxAnaChannel.Text.Split('-');
                        ushort channel = ushort.Parse(lines[1]);
                        int stationId = this.comboBoxStation.SelectedIndex + 4;*/

            int channelId = this.comboBoxAnaChannel.SelectedIndex + 1;
            int stationId = this.comboBoxStation.SelectedIndex + 4;
            int linkChannelId = TestAdapter.ConvertChannelId(stationId, channelId);
            this.AppendText(string.Format("通道切换:{0}:{1}", channelId, linkChannelId));

            bool result = this.TestAdapter.SwitchChannelOperate(stationId, (ushort)linkChannelId, (SwitchMode)this.comboBoxSwitchMode.SelectedIndex);
            this.AppendText(string.Format("通道切换:{0}", result?"成功":"失败"));
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
            this.AppendText(string.Format("设置机械手动作参数[{0}->{1} : {2}]", start, targent, mode));
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.SetRobotMoveEnable();
            this.AppendText(string.Format("设置机械手使能"));
        }

        private void comboBoxStart_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void button13_Click(object sender, EventArgs e)
        {
            string content = this.comboBoxDeviceType.Text;
            string[] list = content.Split('-');
            int value = int.Parse(list[0]);
            this.TestAdapter.VISController.ProgramDeviceType = value;
            content = "";
 
            this.textBoxModuleSerialCode.Text = "";
            bool result = this.TestAdapter.VISController.TryQRModelSerialCode(ref content);
            this.textBoxModuleSerialCode.Text = string.Format("{0}:{1}", result ? "OK" : "Error", content);
        }

        private void button12_Click(object sender, EventArgs e)
        {
            string content = this.comboBoxDeviceType.Text;
            string[] list = content.Split('-');
            int value = int.Parse(list[0]);
            this.TestAdapter.VISController.ProgramDeviceType = value;
            content = "";
            this.textBoxModuleType.Text = "";
            bool result = this.TestAdapter.VISController.TryOCRModelType(ref content);
            this.textBoxModuleType.Text = string.Format("{0}:{1}",result?"OK":"Error",content);

        }

        private void button14_Click(object sender, EventArgs e)
        {
            string content = this.comboBoxDeviceType.Text;
            string[] list = content.Split('-');
            int value = int.Parse(list[0]);
            this.TestAdapter.VISController.ProgramDeviceType = value;
            content = "";


            this.richTextBoxLighting.Text = "";
           int channel = comboBoxModuleChannel.SelectedIndex + 1;
            ModuleType moduleType = (ModuleType)(comboBoxModuleType.SelectedIndex + 1);

            int channelcount = (this.comboBoxChannelCount.SelectedIndex + 1) * 8;
            bool result = this.TestAdapter.VISController.TryOCRChannelLighting( moduleType, channelcount, ref content);
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
            this.button9_Click(null,null);

/*            if (this.TestAdapter.ProcessController.Active())
            {


                this.listView1.BeginUpdate();
                this.listView1.Items.Clear();
                this.AddItem("流控PLC初始化完成", this.TestAdapter.ProcessController.InitializeCompleted);

                this.AddItem("机械手到达位置", this.TestAdapter.ProcessController.RobotMoveCompleted);
                this.AddItem("机械手空闲状态", this.TestAdapter.ProcessController.RobotIdle);
                this.listView1.EndUpdate();
            }*/
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
                this.groupBox5.Text = string.Format("模拟量信号测量器-{0}", this.TestAdapter.MeasureDevice.Identify);
                this.buttonMGet.Visible = true;
                this.checkBox1.Visible = true;
            }
            else
            {
                this.groupBox5.Text = string.Format("模拟量信号测量器-Error");
            }
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Station station = this.TestAdapter.Stations[this.comboBoxStation.SelectedIndex + 3];

            int channelId = this.comboBoxAnaChannel.SelectedIndex + 1;

            float value = 0;
            ChannelType type = (ChannelType)comboBoxGtype.SelectedIndex;
            if (float.TryParse(this.textBoxGValue.Text, out value))
            {
                if( (this.checkBoxCompensate.Checked) && (type == ChannelType.Resistance))
                {
                    value = station.CompensateValue(channelId, value);
                }
                bool result = this.TestAdapter.GeneratorDevice.SetValue(type, value);
                this.AppendText(string.Format("设置{0}[{1}]{2}",type.Description(),value,result?"成功":"失败"));
            }
            
        }

        private void comboBoxGtype_SelectedIndexChanged(object sender, EventArgs e)
        {          
            this.textBoxGValue.Text = "0";
            string[] values = this.comboBoxGtype.Text.Split('-');
            if (values.Length==3)
            {
                this.labelUnit.Text = values[2];
            }


        }

        private void button9_Click(object sender, EventArgs e)
        {
            float value = 0;
            ChannelType type = (ChannelType)comboBoxMType.SelectedIndex;
            if (this.TestAdapter.MeasureDevice.GetValue(type, ref value))
            {
                this.textBoxMValue.Text = value.ToString();
                this.AppendText(string.Format("读取{0}[{1}]成功", type.Description(), value));
            }
            else
            {
                this.textBoxMValue.Text = "Error";
                this.AppendText(string.Format("读取{0}失败", type.Description()));
            }


        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.SetModuleTypeOCRCompleted();
        }

        private void button8_Click_1(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.SetModuleQRCompleted();
        }

        private void button9_Click_1(object sender, EventArgs e)
        {
            this.TestAdapter.ProcessController.SetModuleOCRLightingCompleted();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int channel = this.comboBox1.SelectedIndex + 1;
            this.TestAdapter.SetDigitalChannelValue(0,channel,true);
        }

        private void button16_Click(object sender, EventArgs e)
        {
            int channel = this.comboBox1.SelectedIndex + 1;
            this.TestAdapter.SetDigitalChannelValue(0, channel, false);
        }

        private void button15_Click(object sender, EventArgs e)
        {
            int channel = this.comboBox1.SelectedIndex + 1;
            this.TestAdapter.SetAnalogueChannelValue(3, channel, 3, 0);
        }

        private void button17_Click(object sender, EventArgs e)
        {
            bool result = this.TestAdapter.ProcessController.StartStationTest(this.comboBox2.SelectedIndex+1);
            this.AppendText(string.Format("工位使能:{0}", result ? "成功" : "失败"));
        }

        private void button18_Click(object sender, EventArgs e)
        {
            this.TestAdapter.SetTestResult(this.comboBox2.SelectedIndex + 1,true);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            this.timer.Enabled = this.checkBox1.Checked;
        }

        private void comboBoxStation_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.checkBoxCompensate.Visible = this.comboBoxStation.SelectedIndex == 2 || this.comboBoxStation.SelectedIndex == 3;
        }
    }
}
