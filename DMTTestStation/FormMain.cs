using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TAI.Test.Scheme;
using DMT.DatabaseAdapter;
using TAI.StationManager;
using TAI.TestDispatcher;
using DMT.Core.Utils;
using DMT.Core.Models;

namespace DMTTestStation
{


    public partial class FormMain : Form
    {

        public const int ImageIdleIndex = 0;
        public const int ImageTestingIndex = 1;
        public const int ImageFinishIndex = 2;
        public const int ImageErrorIndex = 3;

        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Program.StationManager = new StationManager();
            Program.StationManager.Initialize();
            Program.StationManager.AttachObserver(this.Update);
        }

        private void RefreshControlUnitTreeViewer()
        {
            this.treeViewCUInfo.BeginUpdate();
            try
            {
                this.treeViewCUInfo.Nodes.Clear();
                ControlUnit CU = Program.StationManager.ControlUnit;
                TreeNode mainNode = this.treeViewCUInfo.Nodes.Add("ControlUnit", Program.StationManager.ControlUnit.Name);
                foreach (var cardGroup in CU.CardGroups)
                {
                    TreeNode cardGroupNode = mainNode.Nodes.Add("CardGroup", cardGroup.Description);
                    cardGroupNode.Tag = cardGroup;
                    foreach (var card in cardGroup.Cards)
                    {
                        TreeNode cardNode = cardGroupNode.Nodes.Add("Card", card.Description);
                        cardNode.Tag = card;
                        cardNode.Checked = false;
                    }
                }
                this.treeViewCUInfo.ExpandAll();
            }
            finally
            {
                this.treeViewCUInfo.EndUpdate();
            }

        
        }

        private void toolStripButtonLoadCUInfo_Click(object sender, EventArgs e)
        {
            if (this.openFileDialogCU.ShowDialog() == DialogResult.OK)
            {
                bool result = Program.StationManager.LoadControlUnit(this.openFileDialogCU.FileName);
                if (result)
                {
                    this.RefreshControlUnitTreeViewer();
                }
                else
                {
                    Windows.MessageBoxError(string.Format("载入控制器配置文件[{0}]失败！", this.openFileDialogCU.FileName));
                }
            }
        }

        private void DeleteCardModule(CardModule card)
        { 
            
        }

        private CardModuleItem FindCardModuleItem(CardModule card)
        {
            for (int i = 0; i < this.listViewCardTest.Items.Count; i++)
            {
                ListViewItem item = this.listViewCardTest.Items[i];
                CardModuleItem cardItem = (CardModuleItem)item.Tag;
                if (cardItem.CardModule.Equals(card))
                {
                    return cardItem;
                }
            }
            return null;
        }

        private void InsertCardModule(CardModule card)
        {
            ListViewItem item = new ListViewItem();
            item.Text = (listViewCardTest.Items.Count+1).ToString();
            item.SubItems.Add(card.Description);
            item.SubItems.Add(card.CardType.ToString());
            item.SubItems.Add("");// 测试进度
            item.SubItems.Add("--");//结果
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.SubItems.Add("");
            item.StateImageIndex = ImageIdleIndex;
            CardModuleItem cardItem = new CardModuleItem(card);
            item.Tag = cardItem;
            this.listViewCardTest.Items.Add(item);

            int rowIndex = listViewCardTest.Items.Count - 1;
            Windows.AddControlToListView(this.listViewCardTest, cardItem.ProgressBar, 3, rowIndex, 1, 0.8f);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.StartButton, 5, rowIndex);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.PauseButton, 6, rowIndex);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.StopButton, 7, rowIndex);
            card.Operator = cardItem;

        }

        private void treeViewCUInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (treeViewCUInfo.SelectedNode.Tag is CardModule)
            {
                CardModule card = (CardModule)treeViewCUInfo.SelectedNode.Tag;
                if (card.Operator == null)
                {
                    this.InsertCardModule(card);
                    Program.StationManager.InitializeModuleTest(card);

                }
            }
        }

        private void RefreshCardModuleItemList()
        {
            try
            {
                this.listViewCardTest.BeginUpdate();

                for (int i = 0; i < this.listViewCardTest.Items.Count; i++)
                {
                    ListViewItem item = this.listViewCardTest.Items[i];
                    item.Text = (i + 1).ToString();
                    CardModuleItem cardItem = (CardModuleItem)item.Tag;
                    int rowIndex = i;
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.ProgressBar, 3, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.StartButton, 5, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.PauseButton, 6, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.StopButton, 7, rowIndex);
                }
            }
            finally
            {
                this.listViewCardTest.EndUpdate();
            }
        }
        private void listViewCardTest_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            this.RefreshCardModuleItemList();
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

        private void RefreshDeviceStatus()
        { 
                 
        }

        private void RefreshTestData(CardModule card)
        {
            CardModuleItem moduleItem = this.FindCardModuleItem(card);
            if (moduleItem !=null)
            {
                moduleItem.ProgressBar.Value = card.TestDispatcher.ProgressValue;

            }
        }

        private void ShowStatus(int Event, string flag, string content, object result, string message, object sender)
        {
            try
            {
                NotifyEvents notifyEvent = (NotifyEvents)Event;
                switch (notifyEvent)
                {
                    case NotifyEvents.Update:
                        {
                            if (flag == "Stauts")
                            {
                                this.RefreshDeviceStatus();
                            }
                            break;
                        }
                    case NotifyEvents.Message:
                        {
                            if (result is CardModule)
                            {
                                this.RefreshTestData((CardModule)result);
                                this.richTextBoxLogs.AppendText(string.Format("{0}:{1}-{2}\r", DateTime.Now.ToString(), flag,message));
                            }
                            break;
                        }
                    case NotifyEvents.Progress:
                        {
                            try
                            {
                                TestingState state = (TestingState)Enum.Parse(typeof(TestingState), flag);
                                if (state == TestingState.Finish) 
                                {
                                    if (result is CardModule)
                                    {
                                        CardModule cardModule = (CardModule)result;
                                        CardModuleItem moduleItem = this.FindCardModuleItem(cardModule);
                                        if (moduleItem != null)
                                        {                                            
                                            moduleItem.StopTest(true);
                                        }
                                    }
                               }
                            }
                            catch
                            { 
                                
                            }
                                                     
                            this.richTextBoxLogs.AppendText(string.Format("{0}:{1}-{2}\r", DateTime.Now.ToString(), flag, message));
                            break;
                        }
                }
            }
            catch (Exception)
            {
                
            }

        }

        private void toolStripButtonStartTest_Click(object sender, EventArgs e)
        {
            if (Windows.MessageBoxQuestion("开始所有板卡测试?"))
            {
                foreach (ListViewItem item in this.listViewCardTest.Items)
                {
                    CardModuleItem cardItem = (CardModuleItem)item.Tag;
                    cardItem.StartTest();
                }
            }
        }

        private void toolStripButtonPauseTest_Click(object sender, EventArgs e)
        {
            if (Windows.MessageBoxQuestion("暂停所有板卡测试?"))
            {
                foreach (ListViewItem item in this.listViewCardTest.Items)
                {
                    CardModuleItem cardItem = (CardModuleItem)item.Tag;
                    cardItem.PauseTest();
                }
            }
        }

        private void toolStripButtonStopTest_Click(object sender, EventArgs e)
        {
            if (Windows.MessageBoxQuestion("停止所有板卡测试?"))
            {
                foreach (ListViewItem item in this.listViewCardTest.Items)
                {
                    CardModuleItem cardItem = (CardModuleItem)item.Tag;
                    cardItem.StopTest();
                }
            }
        }

        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            if (this.listViewCardTest.SelectedItems.Count > 0)
            {
                foreach (ListViewItem item in this.listViewCardTest.SelectedItems)
                {
                    CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                    if (cardItem.CardModule.IsTesting)
                    {
                        Windows.MessageBoxWarning("请先停止测试！");
                        return;
                    }
                }

                foreach (ListViewItem item in this.listViewCardTest.SelectedItems)
                {
                    CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                    Program.StationManager.RemoveModuleTest(cardItem.CardModule);
                    cardItem.Dispose();
                    
                }

                for (int i = this.listViewCardTest.SelectedItems.Count-1; i >= 0; i--)
                {
                    this.listViewCardTest.Items.RemoveAt(this.listViewCardTest.SelectedItems[i].Index);
                }

                this.RefreshCardModuleItemList();
            }
        }

        private void toolStripButtonDeleteAll_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in this.listViewCardTest.Items)
            {
                CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                if (cardItem.CardModule.IsTesting)
                {
                    Windows.MessageBoxWarning("请先停止测试！");
                    return;
                }
            }

            foreach (ListViewItem item in this.listViewCardTest.Items)
            {
                CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                Program.StationManager.RemoveModuleTest(cardItem.CardModule);
                cardItem.Dispose();
            }

            this.listViewCardTest.Items.Clear();

           

        }
    }



    public class CardModuleItem:SubjectObserver
    {
        public Button StartButton { get; set; }
        public Button PauseButton { get; set; }
        public Button StopButton { get; set; }
        public ProgressBar ProgressBar { get; set; }

        public CardModule CardModule { get; set; }

        public CardModuleItem(CardModule card)
        {
            this.CardModule = card;
            this.StartButton = new Button();
            this.StartButton.Text = "开始";
            this.StartButton.Enabled = true;           
            this.StartButton.Click += (EventHandler)this.OnStartClick;
            //this.StartButton.Image = global::DMTTestStation.Properties.Resources.Start;

            this.PauseButton = new Button();
            this.PauseButton.Text = "暂停";
            this.PauseButton.Enabled = false;
            this.PauseButton.Click += (EventHandler)this.OnPauseClick;
            //this.PauseButton.Image = global::DMTTestStation.Properties.Resources.Pause;

            this.StopButton = new Button();
            this.StopButton.Text = "停止";
            this.StopButton.Enabled = false;
            this.StopButton.Click += (EventHandler)this.OnStopClick;
            //this.StopButton.Image = global::DMTTestStation.Properties.Resources.Stop;

            this.ProgressBar = new ProgressBar();
            this.ProgressBar.Value = 0;
            this.ProgressBar.Minimum = 0;
            this.ProgressBar.Maximum = 100;
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
        }


        public void StartTest()
        {
            this.CardModule.TestDispatcher.StartTest();
            this.StartButton.Enabled = false;
            this.PauseButton.Enabled = true;
            this.StopButton.Enabled = true;
        }

        protected void OnStartClick(object sender,EventArgs e)
        {
            this.StartTest();
        }

        public void PauseTest()
        {
            this.CardModule.TestDispatcher.PauseTest();
            this.StartButton.Enabled = true;
            this.PauseButton.Enabled = false;
            this.StopButton.Enabled = true;
        }
        protected void OnPauseClick(object sender,EventArgs e)
        {
            this.PauseTest();
        }

        public void StopTest(bool normal=false)
        {
            if (!normal)
            {
                this.CardModule.TestDispatcher.StopTest();
            }
            this.StartButton.Enabled = true;
            this.PauseButton.Enabled = false;
            this.StopButton.Enabled = false;
        }
        protected void OnStopClick(object sender,EventArgs e)
        {
            this.StopTest();
        }



        


    }




}
