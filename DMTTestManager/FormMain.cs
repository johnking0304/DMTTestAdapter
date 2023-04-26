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
using DMT.Core.Protocols;

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


        private void RefreshEvnStatus()
        {
            this.groupBoxSystem.Text = string.Format("系统状态[{0} ℃   {1} %]",
                Program.DMTTestAdapter.SystemMessage.Temperature,
                Program.DMTTestAdapter.SystemMessage.Humidity);


        }
        private void timer_Tick(object sender, EventArgs e)
        {
            this.RefreshCommStatus();
            this.RefreshProcessControllerStatus();
            this.RefreshStationStatus();
            this.RefreshModulesStatus();
            this.RefreshEvnStatus();
        }

        private void AddNewItemTolistViewStatus(string text, string value)
        {
            
            ListViewItem item = new ListViewItem();
            item.Text = text;
            item.SubItems.Add(value);
            this.listViewStatus.Items.Add(item);
        }


        private void AddNewItemTolistViewModule(string text, string value,string step)
        {
            ListViewItem item = new ListViewItem();
            item.Text = text;
            item.SubItems.Add(value);
            item.SubItems.Add(step);
            this.listViewModules.Items.Add(item);
        }



        private void RefreshModulesStatus()
        {
            this.listViewModules.BeginUpdate();
            try
            {
                this.listViewModules.Items.Clear();

                foreach (Module module in Program.DMTTestAdapter.TestingModules)
                {
                    this.AddNewItemTolistViewModule(module.PositionIndex.ToString(),module.ModuleType.Description(), module.TestStep.Description());
                }
            }
            finally
            {
                this.listViewModules.EndUpdate();
            }

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


        private ListViewItem AddNewItemTolistViewProcess(ModbusItem item,int groupId)
        {
            ListViewItem listitem = this.FindItem(item.Caption);
            if (listitem == null)
            {
                listitem = this.AddNewItemTolistViewProcess(item.Caption, Program.DMTTestAdapter.ProcessController.SystemStatusValues[item.StartAddress].ToString());
                listitem.Group = this.listViewProcessController.Groups[groupId];
                listitem.Tag = item;
            }
            else
            {
                listitem.SubItems[1].Text = Program.DMTTestAdapter.ProcessController.SystemStatusValues[item.StartAddress].ToString();
            }
            return listitem;
        }

        private ListViewItem FindItem(string caption)
        {
            foreach (ListViewItem item in this.listViewProcessController.Items)
            {
                if (item.Text == caption)
                {
                    return item;
                }
            }
            return null;
        }



        private void RefreshProcessControllerStatus()
        {
            this.listViewProcessController.BeginUpdate();
            try
            {
                //this.listViewProcessController.Items.Clear();

                ListViewItem listitem = this.FindItem("系统整体状态");
                if (listitem == null)
                {
                    listitem = this.AddNewItemTolistViewProcess("系统整体状态", Program.DMTTestAdapter.TestState.TestingState.Description());
                    listitem.Group = this.listViewProcessController.Groups[0];
                }
                else
                {
                    listitem.SubItems[1].Text = Program.DMTTestAdapter.TestState.TestingState.Description();
                }


                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.InitializeCompleted,0);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.NewFeedSignal, 0);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.TestDeviceType, 0);

                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.DIStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.DOStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.PIStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.AIStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.AOStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.RTD3StationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.RTD4StationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.TCStationBusy, 1);
                this.AddNewItemTolistViewProcess(Program.DMTTestAdapter.ProcessController.SystemOperator.PrepareStationBusy, 1);


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
