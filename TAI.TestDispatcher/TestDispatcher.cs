using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TAI.Test.Scheme;
using DMT.Core.Models;

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

        public int ProgressValue { get

            {
                if (this.TestScheme.TestItems.Count > 0)
                {
                    double percent = ((this.ActiveTestItemIndex + 1.0) / this.TestScheme.TestItems.Count )* 100.0f; 
                    return (int)Math.Round(percent);
                }
                return 100;
            } }

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
        { 
            return this.TestScheme.LoadSchemeFromDatabase();
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
                this.ActiveTestItem.ProcessConclusion(dataValue);
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
