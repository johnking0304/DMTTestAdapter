using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DMT.Core.Utils;

namespace DMTTestAdapter
{
    public class RecognizeModuleTestState : TestState
    {
        
        public RecognizeModuleTestState(TestAdapter manager) : base(manager)
        {
            this.Caption = "上料状态-模块识别步骤";
            this.TestingState = TestingState.Feeding;
        }

        public override void Initialize()
        {
            this.LastMessage = "进入模块识别";
            LogHelper.LogInfoMsg(this.LastMessage);
        }

        public override void Execute()
        {
           
        }
        public override void StateCheck()
        {
            if (this.Manager.Command == OperateCommand.StartTest)
            {
                this.Manager.Command = OperateCommand.None;
                this.LastMessage = "接收到启动测试命令，转换为系统上料状态";
                LogHelper.LogInfoMsg(this.LastMessage);
                this.Manager.TestState = new FeedingTestState(this.Manager);
            }

        }
    }
}
