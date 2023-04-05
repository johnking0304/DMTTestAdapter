using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MaterialSkin.Controls;
using DMT.Core.Channels;
using TAI.Constants;
using DMT.Core.Utils;
using DMTTestAdapter;
using TAI.Modules;
using TAI.Manager;

namespace DMTTestManager
{
    public partial class FormMain : MaterialForm
    {
        
        public UDPService UDPService { get; set; }
        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            
            this.UDPService = new UDPService();
            this.UDPService.LoadFromFile(Contant.CONFIG);
            this.UDPService.Open();
            this.UDPService.AttachObserver(this.Update);

            Program.DMTTestAdapter = new TestAdapter();


            this.SelectTableControlPage(1);


        }


        public void SelectTableControlPage(int index)
        {
            /*            for (int i = 0; i < this.materialTabControlMain.TabCount; i++)
                        {
                            *//*                if (index == i)
                                            {
                                                materialTabControlMain.TabPages[i].Parent = this.materialTabControlMain;
                                            }
                                            else
                                            {
                                                materialTabControlMain.TabPages[i].Parent = null;
                                            }*//*


                        }*/
            tabPageAbout.Parent = null;
            tabPageDevices.Parent = null;
            tabPageSystem.Parent = materialTabControlMain;
            tabPageLog.Parent = materialTabControlMain;

        }



        public void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            SubjectObserver.FormInvoke update = new SubjectObserver.FormInvoke(this.ShowStatus);
            try
            {
                this.BeginInvoke(update, notifyEvent, flag, content, result, message, sender);
            }
            catch (System.InvalidOperationException)
            {
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {

            }
        }


        private void AppLogText(string message)
        {
            this.richTextBoxLog.AppendText(string.Format("{0}",message));
        
        }

        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            try
            {            
                switch (Event)
                {
                    case UDPClientChannel.UDP_DATA_EVENT:
                        {
                            this.AppLogText(message);
                            break;
                        }               
                }
            }
            catch
            {

            }

        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Program.DMTTestAdapter.Dispose();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.RefreshCommStatus();
            this.RefreshProcessControllerStatus();
            this.RefreshStationStatus();
        }

        private void AddNewItemTolistViewStatus(string text, string value)
        {
            ListViewItem item = new ListViewItem();
            item.Text = text;
            item.SubItems.Add(value);
            this.listViewStatus.Items.Add(item);
        }


        private void RefreshCommStatus()
        {
            this.listViewStatus.BeginUpdate();
            try
            {
                this.listViewStatus.Items.Clear();
                this.AddNewItemTolistViewStatus(Program.DMTTestAdapter.ProcessController.Caption, 
                    Program.DMTTestAdapter.ProcessController.Active() ? "OK" : "Error");
                this.AddNewItemTolistViewStatus(Program.DMTTestAdapter.DigitalDevice.Caption,
                    Program.DMTTestAdapter.DigitalDevice.Active() ? "OK" : "Error");
                this.AddNewItemTolistViewStatus(Program.DMTTestAdapter.VISController.Caption,
                    Program.DMTTestAdapter.VISController.Active() ? "OK" : "Error");
                this.AddNewItemTolistViewStatus(Program.DMTTestAdapter.MeasureDevice.Caption,
                    Program.DMTTestAdapter.MeasureDevice.Active() ? "OK" : "Error");
                this.AddNewItemTolistViewStatus(Program.DMTTestAdapter.GeneratorDevice.Caption,
                    Program.DMTTestAdapter.GeneratorDevice.Active() ? "OK" : "Error");
                foreach (SwitchController switchor in Program.DMTTestAdapter.SwitchControllers)
                {
                    this.AddNewItemTolistViewStatus(switchor.Caption,switchor.Active() ? "OK" : "Error");
                }
            }
            finally
            {
                this.listViewStatus.EndUpdate();
            }

        }


        private ListViewItem AddNewItemTolistViewProcess(string text, string value)
        {
            ListViewItem item = new ListViewItem();
            item.Text = text;
            item.SubItems.Add(value);
            this.listViewProcessController.Items.Add(item);
            return item;
        }

        private void RefreshProcessControllerStatus()
        {
            this.listViewProcessController.BeginUpdate();
            try
            {
                this.listViewProcessController.Items.Clear();

                ListViewItem item =this.AddNewItemTolistViewProcess("系统整体状态", Program.DMTTestAdapter.TestState.TestingState.Description());
                item.Group = this.listViewProcessController.Groups[0];
                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.InitializeCompleted.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.InitializeCompleted.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[0];
                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.NewFeedSignal.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.NewFeedSignal.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[0];


                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.DIStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.DIStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.DOStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.DOStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.PIStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.PIStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.AIStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.AIStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.AOStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.AOStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.RTD3StationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.RTD3StationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.RTD4StationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.RTD4StationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.TCStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.TCStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];

                item = this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.PrepareStationBusy.Caption,
                    Program.DMTTestAdapter.ProcessController.SystemStatusValues[Program.DMTTestAdapter.ProcessController.SystemOperator.PrepareStationBusy.StartAddress].ToString());
                item.Group = this.listViewProcessController.Groups[1];



            }
            finally
            {
                this.listViewProcessController.EndUpdate();
            }
        }

        private void RefreshStationStatus()
        {
            this.listViewStations.BeginUpdate();
            try
            {
                this.listViewStations.Items.Clear();
                foreach (Station station in Program.DMTTestAdapter.Stations)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = station.StationType.Description();
                    item.SubItems.Add(station.TestStep.Description());
                    string module = "";
                    if (station.LinkedModule != null)
                    {
                        module = station.LinkedModule.ModuleType.Description();
                    }
                    else
                    {
                        module = "----";
                        }
                    item.SubItems.Add(module);
                    this.listViewStations.Items.Add(item);
                }                   
            }
            finally
            {
                this.listViewStations.EndUpdate();
            }
        }

        private void FormMain_Shown(object sender, EventArgs e)
        {
            this.timer.Enabled = true;
        }
    }
}
