using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Utils;

namespace TAI.TestDispatcher
{
    public class PauseTestState: TestState
    {
        
        public PauseTestState(TestDispatcher dispatcher) : base(dispatcher)
        {
            this.Caption = "暂停状态";
            this.TestingState = TestingState.Pause;
            this.Dispatcher.Notify((int)NotifyEvents.Progress, this.TestingState.ToString(), "", this.Dispatcher.CardModule, this.LastMessage);

        }

        public override void Initialize()
        {
            this.LastMessage = "进入【暂停状态】，等待恢复启动测试命令";
            LogHelper.LogInfoMsg(this.LastMessage);
            this.Dispatcher.NotifyMessage(this.LastMessage);
        }

        public override void StateCheck()
        {             
            if (this.ActiveCommand == OperateCommand.StartTest)
            {
                this.ActiveCommand = OperateCommand.None;
                this.LastMessage = "接收到恢复启动测试命令，转换为【测试状态】";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);
                this.Dispatcher.TestState = new TestingTestState(this.Dispatcher);
            }
            else if (this.ActiveCommand == OperateCommand.StopTest)
            {
                this.ActiveCommand = OperateCommand.None;
                this.LastMessage = string.Format("停止测试，转换到【结束状态】");
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);
                this.Dispatcher.TestState = new FinishTestState(this.Dispatcher);
            }

        }
    }
}
