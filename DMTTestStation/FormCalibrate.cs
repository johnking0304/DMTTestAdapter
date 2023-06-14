using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DMT.Core.Models;
using DMT.Core.Utils;
using TAI.TestDispatcher;

namespace DMTTestStation
{
    public partial class FormCalibrate : Form
    {
        private bool Completed { get; set; }
        public FormLogs FormLogs { get; set; }
        private CardModule CardModule { get; set; }
        public FormCalibrate(CardModule card)
        {
            InitializeComponent();
            this.CardModule = card;
            this.Completed = false;
            this.FormLogs = new FormLogs(card.Description);
        }

        private void FormCalibrate_Load(object sender, EventArgs e)
        {
            Windows.AddFormToContainer(this.FormLogs,this.groupBoxLog);
            this.RefreshForm();
        }



        private void AddListItem(string Caption ,string Value)
        {
            ListViewItem item = new ListViewItem();
            item.Text = Caption;
            item.SubItems.Add(Value);
            listViewCardModule.Items.Add(item);
        }
        private void RefreshForm()
        {
            this.progressBarCalibrate.Minimum = 0;
            this.progressBarCalibrate.Maximum = 100;

            listViewCardModule.Items.Clear();
            this.AddListItem("序列号",this.CardModule.SerialCode);
            this.AddListItem("类型", this.CardModule.CardType.Description());
            this.AddListItem("列序号", this.CardModule.ColumnPos.ToString());
            this.AddListItem("行序号", this.CardModule.RowPos.ToString());
            this.AddListItem("IP地址",string.Format("192.168.1.{0}", this.CardModule.IPAddress));       
        }


        public void Update(int notifyEvent, string flag, string content, object result, string message, object sender)
        {
            SubjectObserver.FormInvoke update = new SubjectObserver.FormInvoke(this.ShowStatus);
            try
            {
                this.Invoke(update, notifyEvent, flag, content, result, message, sender);
            }
            catch (System.InvalidOperationException ex)
            {
                ex.ToString();
            }
            catch (System.ComponentModel.InvalidAsynchronousStateException)
            {

            }
        }

        private void RefreshCalibrateData(BaseCalibrator calibrator)
        {
            this.LabelProgress.Text = string.Format("{0}", calibrator.ProgressValueContent);
            this.progressBarCalibrate.Value =calibrator.ProgressValue;
            if (calibrator.CalibrateCompleted)
            {
                this.Completed = true;
                this.buttonCancel.Text = "关闭";
            }
        }


        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            try
            {
                NotifyEvents notifyEvent = (NotifyEvents)Event;
                switch (notifyEvent)
                {
                    case NotifyEvents.Message:
                    case NotifyEvents.Progress:
                        {                         
                            this.RefreshCalibrateData((BaseCalibrator)result);
                            break;
                        }
                }
            }
            catch (Exception)
            {

            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (!this.Completed)
            {
                if (Windows.MessageBoxQuestion("正在进行卡件校准，确认停止吗？"))
                {
                    Program.StationManager.CalibrateController.StopCalibrateModule();
                }
                else
                {
                    return;
                }
            }
            this.Close();
            
        }
    }
}
