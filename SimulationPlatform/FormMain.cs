using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.Manager;
using TAI.Device;
using TAI.Constants;
using TAI.Modules;
using DMT.Core.Channels;

namespace SimulationPlatform
{
    public partial class FormMain : Form
    {
        public ProcessController ProcessController { get; set; }
        public DigitalDevice DigitalDevice { get; set; }
        public VISController VISController { get; set; }

        public TCPClientChannel TCPChannel { get; set; } 
        public FormMain()
        {
            InitializeComponent();
        }



        private void buttonSetFeedSignal_Click(object sender, EventArgs e)
        {
            this.ProcessController.SystemOperator.NewFeedSignal.Datas[0] = 1;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.SystemOperator.NewFeedSignal);
        }

        private void Form1_Load(object sender, EventArgs e)
        {


            this.TCPChannel = new TCPClientChannel();
            this.TCPChannel.LoadFromFile(Contant.CONFIG);
            this.TCPChannel.Open();
            
            this.DigitalDevice = new DigitalDevice();
            this.DigitalDevice.LoadFromFile(Contant.DIGITAL_CONFIG);
            this.DigitalDevice.Open();
            this.DigitalDevice.Start();

            this.ProcessController = new ProcessController();
            this.ProcessController.LoadFromFile(Contant.PROCESS_CONFIG);
            this.ProcessController.Open();
            this.ProcessController.Start();

            this.VISController = new VISController();
            this.VISController.LoadFromFile(Contant.VIS_CONFIG);
            this.VISController.Open();
            this.VISController.Start();

            this.comboBoxStation.SelectedIndex = 8;
    

        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.TCPChannel.ReConnect();
            this.TCPChannel.SendCommand("Initialize\r");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.TCPChannel.SendCommand("StartTest\r");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            int channel = this.GetStationSelect();
            if (channel > 0)
            {

                string command = string.Format("StartStationTest,{0}\r", channel);
                this.TCPChannel.SendCommand(command);

            }
        }

            private void button4_Click(object sender, EventArgs e)
        {
            this.ProcessController.RobotOperator.GetIdleStatus.Datas[0] = 1;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.RobotOperator.GetIdleStatus);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            this.ProcessController.RobotOperator.MoveCompletedStatus.Datas[0] = 1;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.RobotOperator.MoveCompletedStatus);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            this.ProcessController.SystemOperator.NewFeedSignal.Datas[0] = 0;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.SystemOperator.NewFeedSignal);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            this.ProcessController.RobotOperator.GetIdleStatus.Datas[0] = 0;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.RobotOperator.GetIdleStatus);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            this.ProcessController.SystemOperator.AIStationBusy.Datas[0] = 1;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.SystemOperator.AIStationBusy);
            this.ProcessController.RobotOperator.MoveCompletedStatus.Datas[0] = 1;
            ProcessController.WriteChannel.WriteModbusItem(this.ProcessController.RobotOperator.MoveCompletedStatus);

        }

        private void button9_Click(object sender, EventArgs e)
        {
            //int channel = this.comboBox1.SelectedIndex + 1;
            int channel = this.GetStationSelect();
            if (channel > 0)
            {
                this.TCPChannel.SendCommand(string.Format("SetTestResult,{0},1\r", channel));
            }
        }

        private void button10_Click(object sender, EventArgs e)
        {
            int channel = this.GetStationSelect();
            if (channel > 0)
            {
                this.TCPChannel.SendCommand(string.Format("SetTestResult,{0},0\r", channel));
            }
        }

        private void button12_Click(object sender, EventArgs e)
        {
            int channel = this.GetStationSelect();
            if (channel > 0)
            {
                this.TCPChannel.SendCommand(string.Format("RequestVISLighting,{0}\r", channel));
            }
        }

        private void radioButton1_Click(object sender, EventArgs e)
        {
            this.GetStationSelect();
        }

        private int GetStationSelect()
        {
            int result = -1;
            foreach (var item in this.groupBoxStations.Controls)
            {
                if (item.GetType() == typeof(RadioButton))
                { 
                     if (((RadioButton)item).Checked)
                    {
                         result = int.Parse(((RadioButton)item).Tag.ToString());
                        this.comboBoxStation.SelectedIndex = result - 1;
                    }
                }
            }
            return result;
       
        }

        private void radioButton2_Click(object sender, EventArgs e)
        {
            GetStationSelect();
        }
    }
}
