﻿using System;
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
            string command = string.Format("StartStationTest,{0}\r", comboBox1.SelectedIndex + 1);
            this.TCPChannel.SendCommand(command);
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
            this.TCPChannel.SendCommand("SetTestResult,4,1\r");
        }

        private void button10_Click(object sender, EventArgs e)
        {
            this.TCPChannel.SendCommand("SetTestResult,4,0\r");
        }
    }
}
