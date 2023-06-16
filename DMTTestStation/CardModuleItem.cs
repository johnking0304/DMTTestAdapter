using DMT.Core.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.StationManager;
using TAI.TestDispatcher;

namespace DMTTestStation
{

    public delegate bool CanStartTest(CardModule module);

    public class CardModuleItem : SubjectObserver
    {
        public const int ImageIdleIndex = 0;
        public const int ImageTestingIndex = 1;
        public const int ImageFinishPassIndex = 2;
        public const int ImageFinishNGIndex = 3;
        public const int ImageStopIndex = 4;

        public ListViewItem ViewItem { get; set; }
        public Button StartButton { get; set; }
        public Button PauseButton { get; set; }
        public Button StopButton { get; set; }
        public ProgressBar ProgressBar { get; set; }


        public TabPage TabPageLogs { get; set; }
        public FormLogs FormLogs { get; set; }


        public CardModule CardModule { get; set; }

        public CanStartTest CanStartTest { get; set; }

        public StationManager Manager { get; set; }

        public CardModuleItem(CardModule card, ListViewItem item,StationManager manager)
        {
            this.Manager = manager;
            this.CardModule = card;
            this.ViewItem = item;
            this.StartButton = new Button();
            this.StartButton.Text = "开始";
            this.StartButton.Enabled = false;
            this.StartButton.ImageAlign = ContentAlignment.MiddleLeft;
            this.StartButton.Click += (EventHandler)this.OnStartClick;
            this.StartButton.Image = Properties.Resources.StartTest;

            this.PauseButton = new Button();
            this.PauseButton.Text = "暂停";
            this.PauseButton.Enabled = false;
            this.PauseButton.ImageAlign = ContentAlignment.MiddleLeft;

            this.PauseButton.Click += (EventHandler)this.OnPauseClick;
            this.PauseButton.Image = Properties.Resources.PauseTest;

            this.StopButton = new Button();
            this.StopButton.Text = "停止";
            this.StopButton.Enabled = false;
            this.StopButton.ImageAlign = ContentAlignment.MiddleLeft;
            this.StopButton.Click += (EventHandler)this.OnStopClick;
            this.StopButton.Image = Properties.Resources.StopTest;

            this.ProgressBar = new ProgressBar();
            this.ProgressBar.Value = 0;
            this.ProgressBar.Minimum = 0;
            this.ProgressBar.Maximum = 100;


            this.FormLogs = new FormLogs(this.CardModule.Description);
            this.CardModule.TestDispatcher.AttachObserver(this.FormLogs.Update);
            this.TabPageLogs = new TabPage();
            Windows.AddFormToContainer(this.FormLogs, this.TabPageLogs);
            this.TabPageLogs.Text = this.CardModule.CardType.ToString();
        }

        public void Dispose()
        {
            this.StopButton.Visible = false;
            this.StartButton.Visible = false;
            this.ProgressBar.Visible = false;
            this.PauseButton.Visible = false;
            this.StopButton.Dispose();
            this.StartButton.Dispose();
            this.ProgressBar.Dispose();
            this.PauseButton.Dispose();
            this.CardModule.TestDispatcher.DetachObserver(this.FormLogs.Update);
            this.FormLogs.Dispose();
            this.TabPageLogs.Parent = null;
            this.TabPageLogs.Dispose();
        }


        public void StartTest()
        {
            if (this.CanStartTest(this.CardModule))
            {
                this.Manager.SelectDevice(this.CardModule);
                this.CardModule.TestDispatcher.StartTest();
                this.StartButton.Enabled = false;
                this.PauseButton.Enabled = true;
                this.StopButton.Enabled = true;
                this.ViewItem.StateImageIndex = ImageTestingIndex;
            }
            else
            {
                Windows.MessageBoxError("信号及采样设备冲突，无法开始测试！");
            }

        }

        protected void OnStartClick(object sender, EventArgs e)
        {
            this.StartTest();
        }

        public void PauseTest()
        {
            if (this.CardModule.TestDispatcher.TestState.TestingState == TestingState.Testing)
            {
                this.CardModule.TestDispatcher.PauseTest();
                this.StartButton.Enabled = true;
                this.PauseButton.Enabled = false;
                this.StopButton.Enabled = true;
                this.ViewItem.StateImageIndex = ImageTestingIndex;

            }
        }
        protected void OnPauseClick(object sender, EventArgs e)
        {
            this.PauseTest();
        }

        public bool InTesting
        {
            get {

                return this.TestingState == TestingState.Testing || this.TestingState == TestingState.Pause;
            }


        }
        public TestingState TestingState {
            get
            {
                if (this.CardModule != null && this.CardModule.TestDispatcher != null && this.CardModule.TestDispatcher.TestState != null)
                {
                    return this.CardModule.TestDispatcher.TestState.TestingState;
                }
                else
                {
                    return TestingState.Idle;
                }
            }
        }

        public void StopTest(bool normal = false)
        {
            if (!normal)
            {
                this.CardModule.TestDispatcher.StopTest();
            }
            this.StartButton.Enabled = !string.IsNullOrEmpty(this.CardModule.SerialCode);
            this.PauseButton.Enabled = false;
            this.StopButton.Enabled = false;
            this.ViewItem.StateImageIndex = ImageStopIndex;
            this.ProgressBar.Value = 0;
            this.ViewItem.SubItems[4].Text = "";
        }
        protected void OnStopClick(object sender, EventArgs e)
        {
            this.StopTest();
        }



    }
}
