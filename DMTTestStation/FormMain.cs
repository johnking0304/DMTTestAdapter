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
using TAI.Modules;

namespace DMTTestStation
{

    public partial class FormMain : Form
    {
        public const int DescriptionColumnIndex = 1;
        public const int ProgressColumnIndex = 3;
        public const int ConclusionColumnIndex = 4;
        public const int StartButtonColumnIndex = 5;
        public const int PauseButtonColumnIndex = 6;
        public const int StopButtonColumnIndex = 7;

        public FormLogs FormMainLogs { get; set; }


        public FormMain()
        {
            InitializeComponent();
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Program.StationManager = new StationManager();
            Program.StationManager.Initialize();
            Program.StationManager.AttachObserver(this.Update);
            this.InitializeFormViewer();
        }


        private void InitializeFormViewer()
        {
            this.FormMainLogs = new FormLogs("ALL");
            Windows.AddFormToContainer(this.FormMainLogs,this.tabPageMain);
            Program.StationManager.AttachObserver(this.FormMainLogs.Update);
        }

        
        private void RefreshControlUnitTreeViewer()
        {
            this.treeViewCUInfo.BeginUpdate();
            try
            {
                this.treeViewCUInfo.Nodes.Clear();
                ControlUnit CU = Program.StationManager.ControlUnit;
                TreeNode mainNode = this.treeViewCUInfo.Nodes.Add("ControlUnit", Program.StationManager.ControlUnit.Name);
                mainNode.ImageIndex = 0;
                mainNode.SelectedImageIndex = 0;
                foreach (var cardGroup in CU.CardGroups)
                {
                    TreeNode cardGroupNode = mainNode.Nodes.Add("CardGroup", cardGroup.Description);
                    cardGroupNode.Tag = cardGroup;
                    cardGroupNode.ImageIndex = 2;
                    cardGroupNode.SelectedImageIndex = 2;
                    foreach (var card in cardGroup.Cards)
                    {
                        TreeNode cardNode = cardGroupNode.Nodes.Add("Card", card.Description);
                        cardNode.Tag = card;
                        cardNode.Checked = false;
                        cardNode.ImageIndex = 3;
                        cardNode.SelectedImageIndex = 1;
                        card.Node = cardNode;
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

            if (this.Testing())
            {
                return;
            }
             
            if (this.openFileDialogCU.ShowDialog() == DialogResult.OK)
            {
                bool result = Program.StationManager.LoadControlUnit(this.openFileDialogCU.FileName);
                if (result)
                {
                    this.DeleteAllCardModule();
                    this.RefreshControlUnitTreeViewer();
                }
                else
                {
                    Windows.MessageBoxError(string.Format("载入控制器配置文件[{0}]失败！", this.openFileDialogCU.FileName));
                }
            }
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

        private void UpdateOperateButtonStatus()
        {
            this.toolStripButtonDeleteAll.Enabled = this.listViewCardTest.Items.Count > 0;
            this.toolStripButtonDelete.Enabled = this.listViewCardTest.SelectedItems.Count > 0;
            this.toolStripButtonPauseTest.Enabled = this.listViewCardTest.Items.Count > 0;
            this.toolStripButtonStartTest.Enabled = this.listViewCardTest.Items.Count > 0;
            this.toolStripButtonStopTest.Enabled = this.listViewCardTest.Items.Count > 0;
            this.toolStripButtonScan.Enabled = this.listViewCardTest.Items.Count > 0;
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
            item.StateImageIndex = CardModuleItem.ImageIdleIndex;
            CardModuleItem cardItem = new CardModuleItem(card,item);
            cardItem.CanStartTest += this.CanStartTest;
            item.Tag = cardItem;
            this.listViewCardTest.Items.Add(item);

            int rowIndex = listViewCardTest.Items.Count - 1;
            Windows.AddControlToListView(this.listViewCardTest, cardItem.ProgressBar, ProgressColumnIndex, rowIndex);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.StartButton, StartButtonColumnIndex, rowIndex);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.PauseButton, PauseButtonColumnIndex, rowIndex);
            Windows.AddControlToListView(this.listViewCardTest, cardItem.StopButton, StopButtonColumnIndex, rowIndex);
            card.Operator = cardItem;

            cardItem.TabPageLogs.Parent = this.tabControlLogs;
        }

        private void InsertCardModuleToListView()
        {
            if (treeViewCUInfo.SelectedNode != null)
            {
                if (treeViewCUInfo.SelectedNode.Tag is CardModule)
                {
                    CardModule card = (CardModule)treeViewCUInfo.SelectedNode.Tag;
                    if (card.Operator == null)
                    {                       
                        Program.StationManager.InitializeModuleTest(card);
                        this.InsertCardModule(card);
                        this.UpdateOperateButtonStatus();

                    }
                }
            }

        }
        private void treeViewCUInfo_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.InsertCardModuleToListView();
        }


        public bool CanStartTest(CardModule module)
        {
            switch (module.CardType)
            {
                case ModuleType.DI:
                case ModuleType.DO:
                case ModuleType.PI:
                case ModuleType.AO:
                    {
                        foreach (ListViewItem item in this.listViewCardTest.Items)
                        {
                            CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                            if (!module.Equals(cardItem.CardModule) && module.CardType == cardItem.CardModule.CardType && cardItem.InTesting)
                            {
                                return false;
                            }
                        }
                        break;
                    }              
                case ModuleType.AI:
                case ModuleType.RTD3:
                case ModuleType.RTD4:
                case ModuleType.TC:
                    {
                        foreach (ListViewItem item in this.listViewCardTest.Items)
                        {
                            CardModuleItem cardItem = (CardModuleItem)(item.Tag);

                            ModuleType[] types = new ModuleType[4] { ModuleType.AI, ModuleType.RTD3,ModuleType.RTD4,ModuleType.TC };
                            if (!module.Equals(cardItem.CardModule) && types.Contains(cardItem.CardModule.CardType) &&  cardItem.InTesting)
                            {
                                return false;
                            }
                        }
                        break;
                    }
            }
            return true;
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

                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.ProgressBar, ProgressColumnIndex, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.StartButton, StartButtonColumnIndex, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.PauseButton, PauseButtonColumnIndex, rowIndex);
                    Windows.UpdateControlToListView(this.listViewCardTest, cardItem.StopButton, StopButtonColumnIndex, rowIndex);

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
            this.toolStripStatusLabelDeviceStatus.Image = Program.StationManager.DeviceStatus ? global::DMTTestStation.Properties.Resources.Connected : global::DMTTestStation.Properties.Resources.Disconnected;
        }

        private void RefreshTestData(CardModule card)
        {
            CardModuleItem moduleItem = this.FindCardModuleItem(card);
            if (moduleItem !=null)
            {
                moduleItem.ProgressBar.Value = card.TestDispatcher.ProgressValue;
                moduleItem.ViewItem.SubItems[ConclusionColumnIndex].Text = card.TestDispatcher.ProgressValueContent;
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
                            if (flag == "Status")
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
                           }
                            break;
                        }
                    case NotifyEvents.Progress:
                        {
                            try
                            {
                                if (result is CardModule)
                                {
                                    CardModule cardModule = (CardModule)result;
                                    CardModuleItem moduleItem = this.FindCardModuleItem(cardModule);
                                    if (moduleItem != null)
                                    {
                                        TestingState state = (TestingState)Enum.Parse(typeof(TestingState), flag);
                                        switch (state)
                                        {
                                            case TestingState.Finish:
                                                {
                                                    moduleItem.StopTest(true);
                                                    break;
                                                }                                          
                                        }
                                        if (cardModule.TestDispatcher.TestCompleted)
                                        {
                                            moduleItem.ViewItem.StateImageIndex = cardModule.TestDispatcher.TestScheme.Conclusion ? CardModuleItem.ImageFinishPassIndex : CardModuleItem.ImageFinishNGIndex;
                                            moduleItem.ViewItem.SubItems[ConclusionColumnIndex].Text = cardModule.TestDispatcher.TestScheme.Conclusion ? "PASS" : "NG";
                                            
                                        }
                                        else
                                        {
                                            moduleItem.ViewItem.SubItems[ConclusionColumnIndex].Text = "--";
                                        }
                                    }
                                }

                             }
                            catch
                            { 
                                
                            }
                                                     
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
                        Windows.MessageBoxWarning("请先停止板卡测试！");
                        return;
                    }
                }

                foreach (ListViewItem item in this.listViewCardTest.SelectedItems)
                {
                    CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                    cardItem.Dispose();
                    Program.StationManager.RemoveModuleTest(cardItem.CardModule);
                                       
                }

                for (int i = this.listViewCardTest.SelectedItems.Count-1; i >= 0; i--)
                {
                    this.listViewCardTest.Items.RemoveAt(this.listViewCardTest.SelectedItems[i].Index);
                }

                this.RefreshCardModuleItemList();
            }
            this.UpdateOperateButtonStatus();
        }

        private bool Testing()
        {
            foreach (ListViewItem item in this.listViewCardTest.Items)
            {
                CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                if (cardItem.CardModule.IsTesting)
                {
                    Windows.MessageBoxWarning("请先停止板卡测试！");
                    return true;
                }
            }
            return false;
        }
        private void DeleteAllCardModule()
        {

            if (this.Testing())
            {
                return;           
            }


            foreach (ListViewItem item in this.listViewCardTest.Items)
            {
                CardModuleItem cardItem = (CardModuleItem)(item.Tag);
                cardItem.Dispose();
                Program.StationManager.RemoveModuleTest(cardItem.CardModule);
                
            }

            this.listViewCardTest.Items.Clear();

        }
        private void toolStripButtonDeleteAll_Click(object sender, EventArgs e)
        {

            this.DeleteAllCardModule();
            this.UpdateOperateButtonStatus();

        }

        private void listViewCardTest_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.UpdateOperateButtonStatus();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.toolStripStatusLabelTime.Text = DateTime.Now.ToString("yyyy-M-d HH:mm:ss");
        }

        private void ToolStripMenuItemInsertToList_Click(object sender, EventArgs e)
        {
            this.InsertCardModuleToListView();
        }

        private void treeViewCUInfo_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                this.contextMenuStripTreeView.Enabled = treeViewCUInfo.SelectedNode != null && treeViewCUInfo.SelectedNode.Tag is CardModule;
            }
        }

        private void toolStripButtonExpand_Click(object sender, EventArgs e)
        {
            this.treeViewCUInfo.ExpandAll();
        }

        private void toolStripButtonCollapse_Click(object sender, EventArgs e)
        {
            this.treeViewCUInfo.CollapseAll();
        }

        private void toolStripButtonScan_Click(object sender, EventArgs e)
        {
            if (this.listViewCardTest.SelectedItems.Count == 1)
            {
                CardModuleItem cardModuleItem = (CardModuleItem)(this.listViewCardTest.SelectedItems[0].Tag);
                CardModule card = cardModuleItem.CardModule;
                FormInput formInput = new FormInput();
                formInput.ShowDialog();
                if (formInput.DialogResult == DialogResult.OK)
                {
                    card.SerialCode = formInput.SerialCode;
                    this.listViewCardTest.SelectedItems[0].SubItems[DescriptionColumnIndex].Text = card.Description;
                    cardModuleItem.StartButton.Enabled = true;
                    ((TreeNode)card.Node).Text = card.Description;
                }
            }
        }
    }







}
