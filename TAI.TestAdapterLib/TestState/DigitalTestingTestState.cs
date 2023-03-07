using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class DigitalTestState: TestState
    {
        
        public DigitalTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "数字量模块测试状态";
            this.TestingState = TestingState.Testing;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入数字量模块测试状态，等待下发测试参数";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Execute()
        {

        }

        public override void StateCheck()
        {
/*            if (this.Manager.)
            {
                this.LastMessage = "接收到启动测试命令，转换为系统上料状态";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new WaitStartTestState(this.Manager);
            }*/
        }
    }
}
