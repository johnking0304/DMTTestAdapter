using DMT.Core.Models;
using System;
using System.Collections.Generic;
using System.IO;
using TAI.Test.Scheme;
using Newtonsoft.Json;
using DMT.DatabaseAdapter;

namespace TAI.TestDispatcher
{




    public enum  OperateCommand
    {
        None = 0,
        StartTest = 1,
        PaseTest = 2,
        StopTest = 3,
    }




    public class TestDispatcher : BaseController
    {
        public CardModule CardModule { get; set; }
        public TestScheme TestScheme { get; set; }

        public int ActiveTestItemIndex { get; set; }

        public int ProgressValue
        {
            get

            {
                if (this.TestScheme.TestItems.Count > 0)
                {
                    double percent = ((this.ActiveTestItemIndex + 1.0) / this.TestScheme.TestItems.Count) * 100.0f;
                    return (int)Math.Round(percent);
                }
                return 100;
            }
        }

        public string ProgressValueContent
        {
            get
            {
                return string.Format("{0}/{1}", this.ActiveTestItemIndex + 1, this.TestScheme.TestItems.Count);
            }
        }

        public bool TestCompleted { get; set; }

        public TestState TestState { get; set; }
        public TestDispatcher(CardModule cardModule)
        {
            this.ActiveTestItemIndex = 0;
            this.CardModule = cardModule;
            this.TestScheme= new TestScheme(this.CardModule.CardType);           
        }

        public void Dispose()
        {
            this.Clear();
        }

        public bool Initialize()
        {   bool reuslt = this.TestScheme.LoadSchemeFromDatabase();

            for (int i = 0; i < this.CardModule.PointNames.Count; i++)
            {
                string nuconLabel = this.CardModule.PointNames[i];
                TestItemNode node = this.TestScheme.TestItems[i];

                if (node.SignalItem.SignalTypeFrom == SignalType.NuCON)
                {
                    node.SignalItem.SignalFromPort = nuconLabel;
                }
                else if (node.SignalItem.SignalTypeTo == SignalType.NuCON)
                {
                    node.SignalItem.SignalToPort = nuconLabel;
                }
            }
            string fileName = string.Format("{0}.ini", this.CardModule.CardType.ToString());
            this.TestScheme.LoadFromFile(Path.Combine(Constants.Contant.CARD_MODULES, fileName));
            return reuslt;
        }

        public TestItemNode ActiveTestItem
        {
            get
            {
                if ((this.ActiveTestItemIndex < this.TestScheme.TestItems.Count) && (this.ActiveTestItemIndex >= 0))
                {
                    return this.TestScheme.TestItems[this.ActiveTestItemIndex];
                }
                return null;
            }
        }

        public void SaveConclusion()
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (TestItemNode node in this.TestScheme.TestItems)
            {
                KeyValuePair<string, string> item = new KeyValuePair<string, string>(node.ReportKey,node.Conclusion?"PASS":"NG");
                result.Add(item);           
            }
            Card card = new Card();
            card.SN = this.CardModule.CageNum;
            card.ProductId = this.CardModule.CardType.ToString();
            card.Tested = true;
            card.IsPassed = this.TestScheme.Conclusion;
            card.TestResult = JsonConvert.SerializeObject(result);
            card.TestTimeStamp = DateTime.Now;

            MysqlSugarContext.MysqlSugarDB.Insertable<Card>(card).ExecuteCommand();
        }


        public void ProcessAction()
        {
            if (this.ActiveTestItem != null)
            {
                string message = "";
                bool result = this.ActiveTestItem.SetChannelValue((int)this.CardModule.CardType, this.ActiveTestItem.SignalItem.SignalFromPort, (int)this.TestScheme.ChannelDataType, this.ActiveTestItem.SignalValue,ref message);
                this.NotifyMessage(message);
   
                this.Delay(this.TestScheme.ProcessIntervalMillisecond);
                this.NotifyMessage(string.Format("等待[{0}]ms", this.TestScheme.ProcessIntervalMillisecond));
                float dataValue = 0;
                this.ActiveTestItem.GetChannelValue((int)this.CardModule.CardType, this.ActiveTestItem.SignalItem.SignalToPort, (int)this.TestScheme.ChannelDataType, ref dataValue,ref message);
                this.NotifyMessage(message);
                this.ActiveTestItem.ProcessConclusion(dataValue,ref message);

                this.NotifyMessage(message);
                message = string.Format("比对结果[{0}]", this.ActiveTestItem.Conclusion ? "PASS" : "NG");
                this.NotifyMessage(message);
                this.ActiveTestItemIndex += 1;
            }
            else
            {
                this.TestCompleted = true;
            }
        }


        public override void ProcessEvent()
        {
            if (this.TestState != null)
            {
                this.TestState.Execute();
            }
        }

        public void NotifyMessage(string message)
        {
            this.Notify((int)NotifyEvents.Message,this.CardModule.Description,"",this.CardModule,message);
        }


        public void Clear()
        {
            
            this.Terminated = false;
            this.ActiveTestItemIndex = 0;
            this.TestCompleted = false;
            foreach (var item in this.TestScheme.TestItems)
            {
                item.Clear();
            }
            this.NotifyMessage("测试板卡数据初始化完成");
        
        }

        public void InitializeWorking()
        {
            this.Clear();
            this.TestState = new IdleTestState(this);
            this.StartThread();

        }


        public void StartTest()
        {
            if (this.TestState != null)
            {
                switch (this.TestState.TestingState)
                {
                    case TestingState.Finish:
                        {
                            this.InitializeWorking();
                            break;
                        }
                    case TestingState.Testing:
                        {
                            return;
                        }
                }
                this.TestState.ActiveCommand = OperateCommand.StartTest;
            }
        }

        public void StopTest()
        {
            if (this.TestState != null)
            {
                this.TestState.ActiveCommand = OperateCommand.StopTest;
            }
        }

        public void PauseTest()
        {
            if (this.TestState != null)
            {
                this.TestState.ActiveCommand = OperateCommand.PaseTest;
            }
        }

    }
}
