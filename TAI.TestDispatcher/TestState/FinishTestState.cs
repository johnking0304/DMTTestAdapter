using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Utils;

namespace TAI.TestDispatcher
{
    public class FinishTestState: TestState
    {
        public bool Operated { get; set; }
        
        public FinishTestState(TestDispatcher dispatcher) : base(dispatcher)
        {
            this.Caption = "结束状态";
            this.TestingState = TestingState.Finish;
            this.Operated = false;
            this.Dispatcher.Notify((int)NotifyEvents.Progress, this.TestingState.ToString(), "", this.Dispatcher.CardModule, this.LastMessage);
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【结束状态】";
            LogHelper.LogInfoMsg(this.LastMessage);
            this.Dispatcher.NotifyMessage(this.LastMessage);
        }

        public override void Execute()
        {
            //处理报告
            if (this.Dispatcher.CardModule.TestCompleted)
            {
                this.Dispatcher.SaveConclusion();
                this.LastMessage = "保存测试数据完成";
                this.Dispatcher.NotifyMessage(this.LastMessage);             
                this.LastMessage = "测试报告处理完成";
                LogHelper.LogInfoMsg(this.LastMessage);
            }
            this.Operated = true;
            this.StateCheck();
        }
        public override void StateCheck()
        {
            if (this.Operated)
            {               
                this.LastMessage = "=========测试结束========";
                this.Dispatcher.NotifyMessage(this.LastMessage);
                this.Dispatcher.StopThread();
            }
        }
    }
}
