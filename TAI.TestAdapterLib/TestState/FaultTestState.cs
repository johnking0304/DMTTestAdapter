using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class FaultTestState: TestState
    {
        
        public FaultTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "系统故障状态";
            this.TestingState = TestingState.Fault;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入系统故障状态";
            LogHelper.LogInfoMsg(this.LastMessage);
        }
        public override void StateCheck()
        {
            if (this.Manager.SystemMessage.LastErrorCode == (int)SystemCode.Ok)
            {
                this.LastMessage = "系统故障恢复，返回到空闲状态";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new IdleTestState(this.Manager);
            }
        }
    }
}
