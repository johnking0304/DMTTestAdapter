using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class IdleTestState: TestState
    {
        
        public IdleTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "空闲状态";
            this.TestingState = TestingState.Idle;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入【空闲状态】，等待下发启动测试命令";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void StateCheck()
        {
            if (this.Manager.Command == OperateCommand.StartTest)
            {
                this.Manager.Command = OperateCommand.None;
                this.LastMessage = "接收到启动测试命令，转换为【系统预上料状态】";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new PreFeedingTestState(this.Manager);
            }

            if (this.Manager.SystemMessage.LastErrorCode == (int)SystemCode.Fault)
            {
                this.Manager.TestState = new FaultTestState(this.Manager);
            }
        }
    }
}
