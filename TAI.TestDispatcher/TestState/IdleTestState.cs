using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Models;
using DMT.Core.Utils;

namespace TAI.TestDispatcher
{
    public class IdleTestState: TestState
    {
        
        public IdleTestState(TestDispatcher dispatcher) : base(dispatcher)
        {
            this.Caption = "空闲状态";
            this.TestingState = TestingState.Idle;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【空闲状态】，等待下发启动测试命令";
            LogHelper.LogInfoMsg(this.LastMessage);

            this.Dispatcher.Notify((int)NotifyEvents.Progress, this.TestingState.ToString(), "", this.Dispatcher.CardModule, this.LastMessage);
        }

        public override void StateCheck()
        {             
            if (this.ActiveCommand == OperateCommand.StartTest)
            {
                this.ActiveCommand = OperateCommand.None;
                this.Dispatcher.NotifyMessage("==========================================================");
                this.LastMessage = "接收到启动测试命令，转换为【测试状态】";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Dispatcher.NotifyMessage(this.LastMessage);
                this.Dispatcher.TestState = new TestingTestState(this.Dispatcher);
            }
        }
    }
}
